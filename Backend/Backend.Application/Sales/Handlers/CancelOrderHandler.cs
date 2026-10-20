using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new DomainException($"Заказ с Id={request.OrderId} не найден");

        if (order.UserId != request.UserId)
            throw new DomainException("Нет доступа к этому заказу");

        order.Cancel("Отменён клиентом");

        await _orderRepository.UpdateAsync(order, cancellationToken);
    }
}