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