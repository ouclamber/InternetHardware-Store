namespace Backend.Application.Sales.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? MainImage { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }

    /// <summary>Сколько товара осталось на складе (для подсказок в корзине).</summary>
    public int StockQuantity { get; set; }

    /// <summary>Товар активен и требуемое количество есть на складе.</summary>
    public bool IsAvailable { get; set; }
}