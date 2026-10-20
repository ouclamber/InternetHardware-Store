using Backend.Domain.Shared;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Sales.ValueObjects;

namespace Backend.Domain.Sales.CartAggregate;

public class CartItem : Entity
{
    public int CartId { get; private set; }
    public int ProductId { get; private set; }
    public Quantity Quantity { get; private set; } = null!;
    public Money UnitPrice { get; private set; } = null!;

    public Money TotalPrice => UnitPrice.Multiply(Quantity.Value);

    // Навигационные свойства
    public Cart? Cart { get; private set; }
    public Product? Product { get; private set; }

    // Для EF Core
    protected CartItem() { }

    public CartItem(Product product, Quantity quantity)
    {
        if (product == null)
            throw new DomainException("Товар обязателен");

        if (quantity == null)
            throw new DomainException("Количество обязательно");

        ProductId = product.Id;
        Quantity = quantity;
        UnitPrice = product.Price;
        Product = product;
    }

    internal void SetCartId(int cartId)
    {
        CartId = cartId;
    }

    internal void IncreaseQuantity(int delta)
    {
        Quantity = Quantity.Add(delta);
    }

    internal void SetQuantity(Quantity newQuantity)
    {
        Quantity = newQuantity;
    }

    internal void UpdateUnitPrice(Money newPrice)
    {
        UnitPrice = newPrice;
    }
}