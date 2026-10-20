namespace Backend.Domain.Sales.OrderAggregate;

public enum OrderStatus
{
    Pending = 1,

    Paid = 2,

    Shipped = 3,

    Delivered = 4,

    Cancelled = 5
}

public static class OrderStatusExtensions
{
    public static string ToRussianString(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Ожидает оплаты",
        OrderStatus.Paid => "Оплачен",
        OrderStatus.Shipped => "Отправлен",
        OrderStatus.Delivered => "Доставлен",
        OrderStatus.Cancelled => "Отменён",
        _ => "Неизвестно"
    };

    public static string ToCode(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Paid => "paid",
        OrderStatus.Shipped => "shipped",
        OrderStatus.Delivered => "delivered",
        OrderStatus.Cancelled => "cancelled",
        _ => "unknown"
    };

    public static OrderStatus FromCode(string code) => code?.ToLowerInvariant() switch
    {
        "pending" => OrderStatus.Pending,
        "paid" => OrderStatus.Paid,
        "shipped" => OrderStatus.Shipped,
        "delivered" => OrderStatus.Delivered,
        "cancelled" => OrderStatus.Cancelled,
        _ => throw new ArgumentException($"Неизвестный статус: {code}", nameof(code))
    };
}