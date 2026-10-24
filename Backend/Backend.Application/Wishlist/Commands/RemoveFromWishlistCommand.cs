using MediatR;

namespace Backend.Application.Wishlist.Commands;

public class RemoveFromWishlistCommand : IRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public RemoveFromWishlistCommand(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
    }
}
