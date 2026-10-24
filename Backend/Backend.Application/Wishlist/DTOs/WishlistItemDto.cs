using Backend.Application.DTOs;

namespace Backend.Application.Wishlist.DTOs;

public class WishlistItemDto
{
    public int ProductId { get; set; }
    public DateTime AddedAt { get; set; }
    public ProductDto Product { get; set; } = new();
}
