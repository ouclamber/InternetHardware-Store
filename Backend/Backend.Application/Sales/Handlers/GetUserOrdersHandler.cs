using MediatR;
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
    private readonly IEncryptionService _encryption;

    public GetUserOrdersHandler(
        IOrderRepository orderRepository,
        IEncryptionService encryption)
    {
        _orderRepository = orderRepository;
        _encryption = encryption;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(
        GetUserOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    private OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber.Value,
            UserId = order.UserId,
            Status = order.Status.ToCode(),
            StatusText = order.Status.ToRussianString(),
            TotalAmount = order.TotalAmount.Amount,
            TotalQuantity = order.TotalQuantity,
            CreatedAt = order.CreatedAt,

            Address = order.ShippingAddress.Street,
            City = order.ShippingAddress.City,
            PostalCode = order.ShippingAddress.PostalCode,
            DeliveryMethod = order.DeliveryMethod,
            PaymentMethod = order.PaymentMethod,
            Comment = order.CustomerComment,

            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice.Amount,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice.Amount
            }).ToList()
        };
    }
}