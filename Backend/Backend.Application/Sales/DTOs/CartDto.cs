namespace Backend.Application.Sales.DTOs;

public class CartDto
{
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}