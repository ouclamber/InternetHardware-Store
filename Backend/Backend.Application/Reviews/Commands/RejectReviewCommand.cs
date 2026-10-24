using MediatR;

namespace Backend.Application.Reviews.Commands;

public class RejectReviewCommand : IRequest
{
    public int ReviewId { get; set; }
    public int AdminUserId { get; set; }

    public RejectReviewCommand(int reviewId, int adminUserId)
    {
        ReviewId = reviewId;
        AdminUserId = adminUserId;
    }
}
