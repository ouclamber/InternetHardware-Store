using MediatR;
using Backend.Application.Wishlist.Commands;
using Backend.Domain.Wishlist.Repositories;

namespace Backend.Application.Wishlist.Handlers;

public class RemoveFromWishlistHandler : IRequestHandler<RemoveFromWishlistCommand>
{
    private readonly IWishlistRepository _wishlistRepository;

    public RemoveFromWishlistHandler(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public async Task Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        // Идемпотентно: удаление отсутствующего товара — не ошибка (DELETE)
        await _wishlistRepository.RemoveAsync(request.UserId, request.ProductId, cancellationToken);
    }
}
