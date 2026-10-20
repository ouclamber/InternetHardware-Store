using MediatR;

namespace Backend.Application.Reviews.Commands;

public class DeleteReviewCommand : IRequest
{
    public int ReviewId { get; set; }
    public int RequestingUserId { get; set; }
    public bool IsAdmin { get; set; }

    public DeleteReviewCommand(int reviewId, int requestingUserId, bool isAdmin)
    {
        ReviewId = reviewId;
        RequestingUserId = requestingUserId;
        IsAdmin = isAdmin;
    }
}