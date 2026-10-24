using MediatR;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Wishlist.DTOs;
using Backend.Application.Wishlist.Queries;
using Backend.Domain.Wishlist.Repositories;

namespace Backend.Application.Wishlist.Handlers;

public class GetWishlistHandler :
    IRequestHandler<GetWishlistQuery, IReadOnlyList<WishlistItemDto>>,
    IRequestHandler<GetWishlistProductIdsQuery, IReadOnlyList<int>>
{
    private readonly IWishlistRepository _wishlistRepository;

    public GetWishlistHandler(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public async Task<IReadOnlyList<WishlistItemDto>> Handle(
        GetWishlistQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _wishlistRepository.GetByUserAsync(request.UserId, cancellationToken);

        return items
            .Where(i => i.Product != null)
            .Select(i => new WishlistItemDto
            {
                ProductId = i.ProductId,
                AddedAt = i.AddedAt,
                Product = ProductMapper.MapToDto(i.Product!)
            })
            .ToList();
    }

    public Task<IReadOnlyList<int>> Handle(
        GetWishlistProductIdsQuery request,
        CancellationToken cancellationToken)
        => _wishlistRepository.GetProductIdsAsync(request.UserId, cancellationToken);
}
