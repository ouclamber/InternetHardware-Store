namespace Backend.Application.Sales.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool CanBeCancelled { get; set; }

    // Контакты покупателя (в БД хранятся зашифрованными)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Доставка (расшифрованные)
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Comment { get; set; }

    // Элементы
    public List<OrderItemDto> Items { get; set; } = new();
}