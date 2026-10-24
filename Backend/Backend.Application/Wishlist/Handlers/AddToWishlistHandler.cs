using MediatR;
using Backend.Application.Wishlist.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using Backend.Domain.Wishlist;
using Backend.Domain.Wishlist.Repositories;

namespace Backend.Application.Wishlist.Handlers;

public class AddToWishlistHandler : IRequestHandler<AddToWishlistCommand>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;

    public AddToWishlistHandler(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
    }

    public async Task Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        // Идемпотентно: повторное добавление — не ошибка
        if (await _wishlistRepository.ExistsAsync(request.UserId, request.ProductId, cancellationToken))
            return;

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Товар с Id={request.ProductId} не найден");

        var count = await _wishlistRepository.CountByUserAsync(request.UserId, cancellationToken);
        if (count >= WishlistItem.MaxItemsPerUser)
            throw new DomainException($"В избранном может быть не более {WishlistItem.MaxItemsPerUser} товаров");

        var item = new WishlistItem(request.UserId, product);

        try
        {
            await _wishlistRepository.AddAsync(item, cancellationToken);
        }
        catch (ConflictException)
        {
            // Параллельный запрос успел добавить тот же товар (уникальный индекс) — результат тот же
        }
    }
}
