using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, int>
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public AddToCartHandler(
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<int> Handle(
        AddToCartCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Загрузить товар
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        if (!product.IsActive)
            throw new DomainException($"Товар '{product.Name.Value}' недоступен для покупки");

        // 2. Загрузить корзину (или создать новую)
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart(request.UserId);
            await _cartRepository.AddAsync(cart, cancellationToken);
        }

        // 3. Добавить товар (проверки внутри Cart.AddItem)
        cart.AddItem(product, request.Quantity);

        // 4. Сохранить изменения
        await _cartRepository.UpdateAsync(cart, cancellationToken);

        // 5. Найти CartItem и вернуть его Id
        var item = cart.GetItem(request.ProductId);

        return item?.Id ?? 0;
    }
}