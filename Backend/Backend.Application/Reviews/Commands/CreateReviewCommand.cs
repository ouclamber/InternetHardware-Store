using MediatR;

namespace Backend.Application.Reviews.Commands;

public class CreateReviewCommand : IRequest<int>
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;

    public CreateReviewCommand(int userId, int productId, int rating, string comment)
    {
        UserId = userId;
        ProductId = productId;
        Rating = rating;
        Comment = comment;
    }
}