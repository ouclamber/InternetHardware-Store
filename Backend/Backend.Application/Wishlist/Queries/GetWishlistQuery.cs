using MediatR;
using Backend.Application.Wishlist.DTOs;

namespace Backend.Application.Wishlist.Queries;

public class GetWishlistQuery : IRequest<IReadOnlyList<WishlistItemDto>>
{
    public int UserId { get; set; }

    public GetWishlistQuery(int userId)
    {
        UserId = userId;
    }
}

/// <summary>Только Id товаров в избранном — для отметок в каталоге.</summary>
public class GetWishlistProductIdsQuery : IRequest<IReadOnlyList<int>>
{
    public int UserId { get; set; }

    public GetWishlistProductIdsQuery(int userId)
    {
        UserId = userId;
    }
}
