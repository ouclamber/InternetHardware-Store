using MediatR;

namespace Backend.Application.Reviews.Commands;

public class ApproveReviewCommand : IRequest
{
    public int ReviewId { get; set; }
    public int AdminUserId { get; set; }

    public ApproveReviewCommand(int reviewId, int adminUserId)
    {
        ReviewId = reviewId;
        AdminUserId = adminUserId;
    }
}