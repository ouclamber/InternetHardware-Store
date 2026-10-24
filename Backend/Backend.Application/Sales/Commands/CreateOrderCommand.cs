using MediatR;

namespace Backend.Application.Sales.Commands;

public class CreateOrderCommand : IRequest<int>
{
    public int UserId { get; set; }

    // Контактные данные (для заказа)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Адрес доставки
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }

    // Доставка и оплата
    public string DeliveryMethod { get; set; } = "courier";
    public string PaymentMethod { get; set; } = "card";
    public string? Comment { get; set; }
}