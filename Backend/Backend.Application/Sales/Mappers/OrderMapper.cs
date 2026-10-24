using Backend.Application.Sales.DTOs;
using Backend.Domain.Sales.OrderAggregate;

namespace Backend.Application.Sales.Mappers;

/// <summary>
/// Единый маппинг заказа в DTO (раньше дублировался в двух обработчиках).
/// </summary>
internal static class OrderMapper
{
    public static OrderDto MapToDto(Order order)
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
            PaidAt = order.PaidAt,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            CancelledAt = order.CancelledAt,
            CanBeCancelled = order.CanBeCancelled,

            FirstName = order.Contact?.FirstName,
            LastName = order.Contact?.LastName,
            Email = order.Contact?.Email,
            Phone = order.Contact?.Phone,

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
