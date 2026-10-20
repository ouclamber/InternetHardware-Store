using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new DomainException($"Заказ с Id={request.OrderId} не найден");

        switch (request.NewStatus.ToLowerInvariant())
        {
            case "paid":
                order.MarkAsPaid();
                break;

            case "shipped":
                order.Ship();
                break;

            case "delivered":
                order.Deliver();
                break;

            case "cancelled":
                order.Cancel($"Отменён администратором (Id={request.AdminUserId})");
                break;

            default:
                throw new DomainException($"Недопустимый статус: {request.NewStatus}");
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);
    }
}