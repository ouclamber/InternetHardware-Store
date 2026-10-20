using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class UpdateCartQuantityHandler : IRequestHandler<UpdateCartQuantityCommand>
{
    private readonly ICartRepository _cartRepository;

    public UpdateCartQuantityHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Handle(
        UpdateCartQuantityCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart == null)
            throw new DomainException("Корзина не найдена");

        cart.UpdateItemQuantity(request.ProductId, request.NewQuantity);

        await _cartRepository.UpdateAsync(cart, cancellationToken);
    }
}