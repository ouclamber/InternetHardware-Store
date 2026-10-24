using MediatR;

namespace Backend.Application.Reviews.Commands;

public class UpdateReviewCommand : IRequest
{
    public int ReviewId { get; set; }
    public int RequestingUserId { get; set; }
    public bool IsAdmin { get; set; }
    public int NewRating { get; set; }
    public string NewComment { get; set; } = string.Empty;

    public UpdateReviewCommand(
        int reviewId,
        int requestingUserId,
        bool isAdmin,
        int newRating,
        string newComment)
    {
        ReviewId = reviewId;
        RequestingUserId = requestingUserId;
        IsAdmin = isAdmin;
        NewRating = newRating;
        NewComment = newComment;
    }
}