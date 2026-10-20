using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand>
{
    private readonly ICartRepository _cartRepository;

    public RemoveFromCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Handle(
        RemoveFromCartCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
            throw new DomainException("Корзина не найдена");

        cart.RemoveItem(request.ProductId);

        await _cartRepository.UpdateAsync(cart, cancellationToken);
    }
}