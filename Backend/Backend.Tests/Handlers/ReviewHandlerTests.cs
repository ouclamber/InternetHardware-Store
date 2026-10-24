using Backend.Application.Reviews.Commands;
using Backend.Application.Reviews.Handlers;
using Backend.Application.Reviews.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class ReviewHandlerTests
{
    private readonly Mock<IReviewRepository> _reviewRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();

    private static Review CreateReview(int userId = 1, int productId = 1, int rating = 5)
    {
        var review = new Review(
            userId,
            productId,
            Rating.Of(rating),
            Comment.Create("Отличный товар, рекомендую!"),
            autoApprove: true);

        var idProp = typeof(Review).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(review, 1);

        return review;
    }

    private static Product CreateProduct(int id = 1)
    {
        var product = new Product(
            ProductName.Create("Test Product"),
            Money.Rub(1000),
            brandId: 1,
            categoryId: 1,
            typeId: 1);

        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    [Fact]
    public async Task GetProductReviews_Existing_ReturnsList()
    {
        var handler = new GetProductReviewsHandler(_reviewRepo.Object);
        var reviews = new List<Review> { CreateReview(), CreateReview() };

        _reviewRepo.Setup(r => r.GetByProductAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(reviews);
        _reviewRepo.Setup(r => r.GetCountByProductAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(2);
        _reviewRepo.Setup(r => r.GetAverageRatingAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(5.0);

        var result = await handler.Handle(new GetProductReviewsQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProductReviews_Empty_ReturnsEmptyList()
    {
        var handler = new GetProductReviewsHandler(_reviewRepo.Object);
        _reviewRepo.Setup(r => r.GetByProductAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<Review>());
        _reviewRepo.Setup(r => r.GetCountByProductAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(0);
        _reviewRepo.Setup(r => r.GetAverageRatingAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(0);

        var result = await handler.Handle(new GetProductReviewsQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateReview_ValidData_ReturnsId()
    {
        var handler = new CreateReviewHandler(_reviewRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateProduct());
        _reviewRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);
        _reviewRepo.Setup(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var command = new CreateReviewCommand(1, 1, 5, "Отличный товар, рекомендую!");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _reviewRepo.Verify(r => r.AddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateReview_ProductNotFound_ThrowsDomainException()
    {
        var handler = new CreateReviewHandler(_reviewRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new CreateReviewCommand(1, 999, 5, "Отличный товар, рекомендую!");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateReview_AlreadyExists_ThrowsDomainException()
    {
        var handler = new CreateReviewHandler(_reviewRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateProduct());
        _reviewRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true);

        var command = new CreateReviewCommand(1, 1, 5, "Отличный товар, рекомендую!");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateReview_InvalidRating_ThrowsDomainException()
    {
        var handler = new CreateReviewHandler(_reviewRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateProduct());
        _reviewRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var command = new CreateReviewCommand(1, 1, 10, "Отличный товар, рекомендую!");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateReview_ShortComment_ThrowsDomainException()
    {
        var handler = new CreateReviewHandler(_reviewRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(CreateProduct());
        _reviewRepo.Setup(r => r.ExistsAsync(1, 1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(false);

        var command = new CreateReviewCommand(1, 1, 5, "Ok");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateReview_AuthorUpdates_Updates()
    {
        var handler = new UpdateReviewHandler(_reviewRepo.Object);
        var review = CreateReview(userId: 1);
        _reviewRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(review);
        _reviewRepo.Setup(r => r.UpdateAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        // (reviewId, requestingUserId, isAdmin, newRating, newComment)
        var command = new UpdateReviewCommand(1, 1, false, 3, "Изменил мнение о товаре");
        await handler.Handle(command, CancellationToken.None);

        _reviewRepo.Verify(r => r.UpdateAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReview_AdminUpdatesOtherUser_Updates()
    {
        var handler = new UpdateReviewHandler(_reviewRepo.Object);
        var review = CreateReview(userId: 1);
        _reviewRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(review);
        _reviewRepo.Setup(r => r.UpdateAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var command = new UpdateReviewCommand(1, 99, true, 3, "Изменил мнение о товаре");
        await handler.Handle(command, CancellationToken.None);

        _reviewRepo.Verify(r => r.UpdateAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateReview_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateReviewHandler(_reviewRepo.Object);
        _reviewRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Review?)null);

        var command = new UpdateReviewCommand(999, 1, false, 3, "Изменил мнение о товаре");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateReview_OtherUserNotAdmin_ThrowsDomainException()
    {
        var handler = new UpdateReviewHandler(_reviewRepo.Object);
        var review = CreateReview(userId: 1);
        _reviewRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(review);

        var command = new UpdateReviewCommand(1, 99, false, 3, "Изменил мнение о товаре");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteReview_AuthorDeletes_Deletes()
    {
        var handler = new DeleteReviewHandler(_reviewRepo.Object);
        var review = CreateReview(userId: 1);
        _reviewRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(review);
        _reviewRepo.Setup(r => r.DeleteAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        // (reviewId, requestingUserId, isAdmin)
        await handler.Handle(new DeleteReviewCommand(1, 1, false), CancellationToken.None);

        _reviewRepo.Verify(r => r.DeleteAsync(review, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReview_NotFound_ThrowsDomainException()
    {
        var handler = new DeleteReviewHandler(_reviewRepo.Object);
        _reviewRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Review?)null);

        var act = async () => await handler.Handle(new DeleteReviewCommand(999, 1, false), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteReview_OtherUserNotAdmin_ThrowsDomainException()
    {
        var handler = new DeleteReviewHandler(_reviewRepo.Object);
        var review = CreateReview(userId: 1);
        _reviewRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(review);

        var act = async () => await handler.Handle(new DeleteReviewCommand(1, 99, false), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}