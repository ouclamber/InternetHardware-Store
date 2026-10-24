using Backend.Domain.Reviews;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ReviewModerationTests
{
    private static Review Approved() =>
        new(1, 1, Rating.Of(5), Comment.Create("Отличный товар!"), autoApprove: true);

    private static Review Pending() =>
        new(1, 1, Rating.Of(5), Comment.Create("Отличный товар!"), autoApprove: false);

    [Fact]
    public void Edit_WithoutReset_KeepsApproved()
    {
        var review = Approved();

        review.Edit(Rating.Of(4), Comment.Create("Хороший товар, но шумный"));

        review.Status.Should().Be(ReviewStatus.Approved);
        review.Rating.Value.Should().Be(4);
    }

    [Fact]
    public void Edit_WithReset_ApprovedGoesBackToPending()
    {
        var review = Approved();

        review.Edit(Rating.Of(1), Comment.Create("Передумал, товар плохой"), resetModeration: true);

        review.Status.Should().Be(ReviewStatus.Pending);
        review.ModeratedAt.Should().BeNull();
        review.ModeratedByUserId.Should().BeNull();
    }

    [Fact]
    public void Edit_WithReset_PendingStaysPending()
    {
        var review = Pending();

        review.Edit(Rating.Of(3), Comment.Create("Нормально, без восторга"), resetModeration: true);

        review.Status.Should().Be(ReviewStatus.Pending);
    }

    [Fact]
    public void Edit_Rejected_Throws()
    {
        var review = Pending();
        review.Reject(7);

        var act = () => review.Edit(Rating.Of(3), Comment.Create("Попробую ещё раз"), resetModeration: true);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Pending_ThenApprove_ByAdmin()
    {
        var review = Pending();

        review.Approve(7);

        review.IsApproved.Should().BeTrue();
        review.ModeratedByUserId.Should().Be(7);
    }
}
