using MediatR;
using Backend.Application.Sales.Mappers;
using Backend.Application.Sales.DTOs;
using Backend.Application.Sales.Queries;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class GetUserOrdersHandler 
    : IRequestHandler<GetUserOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetUserOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetUserOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return orders.Select(OrderMapper.MapToDto).ToList();
    }
}
