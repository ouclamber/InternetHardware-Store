using Backend.Domain.Shared;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Sales.ValueObjects;

namespace Backend.Domain.Sales.CartAggregate;

public class Cart : Entity, IAggregateRoot
{
    public const int MaxUniqueItems = 50;

    public int UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public Money TotalAmount => _items
        .Select(i => i.TotalPrice)
        .Aggregate(Money.Zero, (sum, price) => sum.Add(price));

    public int TotalQuantity => _items.Sum(i => i.Quantity.Value);

    protected Cart() { }

    public Cart(int userId)
    {
        if (userId <= 0)
            throw new DomainException("Id пользователя обязателен");

        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(Product product, int quantity)
    {
        if (product == null)
            throw new DomainException("Товар не указан");

        var qty = Quantity.Of(quantity);
        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem == null && _items.Count >= MaxUniqueItems)
            throw new DomainException($"В корзине не может быть больше {MaxUniqueItems} уникальных товаров");

        // Итоговое количество в корзине не должно превышать остаток на складе
        var resultingQuantity = (existingItem?.Quantity.Value ?? 0) + qty.Value;
        product.EnsureCanBePurchased(resultingQuantity);

        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
            existingItem.UpdateUnitPrice(product.Price);
        }
        else
        {
            var cartItem = new CartItem(product, qty);
            _items.Add(cartItem);
        }

        MarkAsUpdated();
    }

    public void UpdateItemQuantity(int productId, int newQuantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Товар с Id={productId} не найден в корзине");

        if (newQuantity <= 0)
        {
            _items.Remove(item);
        }
        else
        {
            var qty = Quantity.Of(newQuantity);

            // Если товар загружен вместе с корзиной — проверяем остаток и актуализируем цену
            if (item.Product != null)
            {
                item.Product.EnsureCanBePurchased(qty.Value);
                item.UpdateUnitPrice(item.Product.Price);
            }

            item.SetQuantity(qty);
        }

        MarkAsUpdated();
    }

    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Товар с Id={productId} не найден в корзине");

        _items.Remove(item);
        MarkAsUpdated();
    }

    public void Clear()
    {
        _items.Clear();
        MarkAsUpdated();
    }

    public bool IsEmpty => !_items.Any();

    public bool ContainsProduct(int productId)
    {
        return _items.Any(i => i.ProductId == productId);
    }

    public CartItem? GetItem(int productId)
    {
        return _items.FirstOrDefault(i => i.ProductId == productId);
    }

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}