using MediatR;

namespace Backend.Application.Wishlist.Commands;

public class AddToWishlistCommand : IRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public AddToWishlistCommand(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
    }
}
