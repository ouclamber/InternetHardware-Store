using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.Repositories;

namespace Backend.Application.Sales.Handlers;

public class ClearCartHandler : IRequestHandler<ClearCartCommand>
{
    private readonly ICartRepository _cartRepository;

    public ClearCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task Handle(
        ClearCartCommand request,
        CancellationToken cancellationToken)
    {
        await _cartRepository.ClearByUserIdAsync(request.UserId, cancellationToken);
    }
}