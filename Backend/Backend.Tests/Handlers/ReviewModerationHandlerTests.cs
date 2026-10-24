using Backend.Application.Reviews;
using Backend.Application.Reviews.Commands;
using Backend.Application.Reviews.Handlers;
using Backend.Application.Reviews.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using Backend.Tests.TestHelpers;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class ReviewModerationHandlerTests
{
    private readonly Mock<IReviewRepository> _reviews = new();
    private readonly Mock<IProductRepository> _products = new();

    private static Review CreateReview(int userId = 1, int rating = 5, bool approved = true) =>
        new Review(userId, 1, Rating.Of(rating), Comment.Create("Комментарий к товару"), autoApprove: approved).WithId(10);

    [Fact]
    public async Task Create_ModerationEnabled_ReviewIsPending()
    {
        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product());
        Review? saved = null;
        _reviews.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                .Callback<Review, CancellationToken>((r, _) => saved = r)
                .Returns(Task.CompletedTask);

        var handler = new CreateReviewHandler(_reviews.Object, _products.Object, new ReviewOptions { RequireModeration = true });
        await handler.Handle(new CreateReviewCommand(1, 1, 5, "Отличный товар, советую"), CancellationToken.None);

        saved!.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public async Task Create_ModerationDisabled_ReviewIsApproved()
    {
        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product());
        Review? saved = null;
        _reviews.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                .Callback<Review, CancellationToken>((r, _) => saved = r)
                .Returns(Task.CompletedTask);

        var handler = new CreateReviewHandler(_reviews.Object, _products.Object);
        await handler.Handle(new CreateReviewCommand(1, 1, 5, "Отличный товар, советую"), CancellationToken.None);

        saved!.Status.Should().Be(ReviewStatus.Approved);
    }

    [Fact]
    public async Task Create_InactiveProduct_Throws()
    {
        _products.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product(active: false));

        var act = async () => await new CreateReviewHandler(_reviews.Object, _products.Object)
            .Handle(new CreateReviewCommand(1, 1, 5, "Отличный товар, советую"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _reviews.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_ByAuthor_WithModeration_ReturnsToPending()
    {
        var review = CreateReview(userId: 1);
        _reviews.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        var handler = new UpdateReviewHandler(_reviews.Object, new ReviewOptions { RequireModeration = true });
        await handler.Handle(new UpdateReviewCommand(10, 1, false, 2, "Поменял мнение о товаре"), CancellationToken.None);

        review.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public async Task Update_ByAdmin_WithModeration_StaysApproved()
    {
        var review = CreateReview(userId: 1);
        _reviews.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        var handler = new UpdateReviewHandler(_reviews.Object, new ReviewOptions { RequireModeration = true });
        await handler.Handle(new UpdateReviewCommand(10, 99, true, 4, "Исправлена опечатка в тексте"), CancellationToken.None);

        review.Status.Should().Be(ReviewStatus.Approved);
    }

    [Fact]
    public async Task Update_ByStranger_ThrowsForbidden()
    {
        _reviews.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(CreateReview(userId: 1));

        var act = async () => await new UpdateReviewHandler(_reviews.Object)
            .Handle(new UpdateReviewCommand(10, 2, false, 1, "Чужой отзыв не трогать"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Reject_PendingReview_Rejects()
    {
        var review = CreateReview(approved: false);
        _reviews.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(review);

        await new RejectReviewHandler(_reviews.Object).Handle(new RejectReviewCommand(10, 7), CancellationToken.None);

        review.Status.Should().Be(ReviewStatus.Rejected);
        review.ModeratedByUserId.Should().Be(7);
        _reviews.Verify(r => r.UpdateAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Reject_NotFound_ThrowsNotFound()
    {
        var act = async () => await new RejectReviewHandler(_reviews.Object).Handle(new RejectReviewCommand(404, 7), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetPending_ReturnsDtos()
    {
        _reviews.Setup(r => r.GetPendingAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { CreateReview(approved: false) });

        var result = await new GetPendingReviewsHandler(_reviews.Object).Handle(new GetPendingReviewsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Status.Should().Be("pending");
    }

    [Fact]
    public async Task GetProductReviews_ReturnsRatingDistribution()
    {
        _reviews.Setup(r => r.GetByProductAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { CreateReview(rating: 5), CreateReview(rating: 5), CreateReview(rating: 2) });

        var result = await new GetProductReviewsHandler(_reviews.Object).Handle(new GetProductReviewsQuery(1), CancellationToken.None);

        result.RatingDistribution[5].Should().Be(2);
        result.RatingDistribution[2].Should().Be(1);
        result.RatingDistribution[1].Should().Be(0);
        result.RatingDistribution.Should().HaveCount(5);
        result.AverageRating.Should().Be(4.0);
    }
}
