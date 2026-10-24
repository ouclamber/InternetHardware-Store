using MediatR;
using Backend.Application.Sales.Mappers;
using Backend.Application.Sales.DTOs;
using Backend.Application.Sales.Queries;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
            return null;

        // Проверка прав: либо админ, либо владелец заказа
        if (!request.IsAdmin && order.UserId != request.RequestingUserId)
            throw new ForbiddenException("Нет доступа к этому заказу");

        return OrderMapper.MapToDto(order);
    }
}
