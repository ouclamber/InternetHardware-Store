using MediatR;
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
            throw new DomainException("Нет доступа к этому заказу");

        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
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