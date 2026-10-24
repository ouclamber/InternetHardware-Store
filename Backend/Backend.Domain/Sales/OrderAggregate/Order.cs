using Backend.Domain.Shared;
using Backend.Domain.Sales.ValueObjects;

namespace Backend.Domain.Sales.OrderAggregate;

public class Order : Entity, IAggregateRoot
{
    public int UserId { get; private set; }
    public OrderNumber OrderNumber { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;

    // Данные доставки (сохраняем на момент покупки)
    public Address ShippingAddress { get; private set; } = null!;
    public string? DeliveryMethod { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? CustomerComment { get; private set; }

    /// <summary>Контакты покупателя (null — для заказов, созданных до появления поля).</summary>
    public CustomerContact? Contact { get; private set; }

    /// <summary>
    /// Под заказ списан товар со склада. При отмене остатки возвращаются
    /// только если флаг установлен (старые заказы ничего не резервировали).
    /// </summary>
    public bool StockReserved { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Для EF Core
    protected Order() { }

    public Order(
        int userId,
        Address shippingAddress,
        IEnumerable<(int ProductId, string ProductName, int Quantity, Money UnitPrice)> items,
        string? deliveryMethod = null,
        string? paymentMethod = null,
        string? comment = null,
        CustomerContact? contact = null)
    {
        if (userId <= 0)
            throw new DomainException("Id пользователя обязателен");

        if (shippingAddress == null)
            throw new DomainException("Адрес доставки обязателен");

        if (items == null)
            throw new DomainException("Список товаров обязателен");

        var itemsList = items.ToList();
        if (!itemsList.Any())
            throw new DomainException("Заказ не может быть пустым");

        if (itemsList.Count > 100)
            throw new DomainException("В одном заказе не может быть больше 100 позиций");

        UserId = userId;
        OrderNumber = OrderNumber.Generate();
        Status = OrderStatus.Pending;
        ShippingAddress = shippingAddress;
        DeliveryMethod = deliveryMethod?.Trim();
        PaymentMethod = paymentMethod?.Trim();
        CustomerComment = comment?.Trim();
        Contact = contact;
        CreatedAt = DateTime.UtcNow;

        // Создаём элементы заказа
        foreach (var item in itemsList)
        {
            var orderItem = new OrderItem(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.UnitPrice);
            _items.Add(orderItem);
        }

        // Фиксируем итоговую сумму
        TotalAmount = _items
            .Select(i => i.TotalPrice)
            .Aggregate(Money.Zero, (sum, price) => sum.Add(price));
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException(
                $"Оплатить можно только заказ в статусе 'Ожидает оплаты'. Текущий статус: '{Status.ToRussianString()}'");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Ship()
    {
        if (Status != OrderStatus.Paid)
            throw new DomainException(
                $"Отправить можно только оплаченный заказ. Текущий статус: '{Status.ToRussianString()}'");

        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new DomainException(
                $"Доставить можно только отправленный заказ. Текущий статус: '{Status.ToRussianString()}'");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == OrderStatus.Delivered)
            throw new DomainException("Нельзя отменить доставленный заказ");

        if (Status == OrderStatus.Cancelled)
            throw new DomainException("Заказ уже отменён");

        if (Status == OrderStatus.Shipped)
            throw new DomainException("Нельзя отменить отправленный заказ");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(reason))
            CustomerComment = $"{CustomerComment}\n[Отмена] {reason}".Trim();

        MarkAsUpdated();
    }

    /// <summary>
    /// Отметить, что товары заказа списаны со склада.
    /// </summary>
    public void MarkStockReserved()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Резервировать товар можно только для нового заказа");

        StockReserved = true;
    }

    /// <summary>
    /// Для отменённого заказа вернуть позиции, которые нужно вернуть на склад,
    /// и снять флаг резерва (повторный вызов вернёт пустой список — двойного возврата не будет).
    /// </summary>
    public IReadOnlyList<(int ProductId, int Quantity)> TakeStockToRelease()
    {
        if (Status != OrderStatus.Cancelled)
            throw new DomainException("Вернуть товар на склад можно только для отменённого заказа");

        if (!StockReserved)
            return Array.Empty<(int, int)>();

        StockReserved = false;

        return _items
            .GroupBy(i => i.ProductId)
            .Select(g => (g.Key, g.Sum(i => i.Quantity)))
            .ToList();
    }

    public void SetShippingAddressFromStorage(Address address)
    {
        ShippingAddress = address ?? throw new DomainException("Адрес не может быть null");
    }

    public bool IsPending => Status == OrderStatus.Pending;
    public bool IsPaid => Status == OrderStatus.Paid;
    public bool IsShipped => Status == OrderStatus.Shipped;
    public bool IsDelivered => Status == OrderStatus.Delivered;
    public bool IsCancelled => Status == OrderStatus.Cancelled;

    public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Paid;

    public int TotalQuantity => _items.Sum(i => i.Quantity);

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}