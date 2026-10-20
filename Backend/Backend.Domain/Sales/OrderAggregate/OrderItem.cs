using Backend.Domain.Shared;
using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Sales.OrderAggregate;

public class OrderItem : Entity
{
    public int OrderId { get; private set; }
    public int ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    // Навигационные
    public Order? Order { get; private set; }
    public Product? Product { get; private set; }

    // Для EF Core
    protected OrderItem() { }

    internal OrderItem(int productId, string productName, int quantity, Money unitPrice)
    {
        if (productId <= 0)
            throw new DomainException("Id товара обязателен");

        if (string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Название товара обязательно");

        if (quantity <= 0)
            throw new DomainException("Количество должно быть больше 0");

        if (unitPrice == null)
            throw new DomainException("Цена товара обязательна");

        if (unitPrice.IsZero)
            throw new DomainException("Цена товара должна быть больше 0");

        ProductId = productId;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void SetOrderId(int orderId)
    {
        OrderId = orderId;
    }
}