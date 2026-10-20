using Backend.Domain.Shared;
using System.Text.RegularExpressions;

namespace Backend.Domain.Sales.ValueObjects;

public sealed class OrderNumber : ValueObject
{
    private static readonly Regex FormatRegex = new(
        @"^ORD-\d{8}-[A-F0-9]{8}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private OrderNumber(string value)
    {
        Value = value;
    }

    public static OrderNumber FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Номер заказа обязателен");

        var trimmed = value.Trim();

        if (!FormatRegex.IsMatch(trimmed))
            throw new DomainException($"Неверный формат номера заказа: {trimmed}. Ожидается ORD-YYYYMMDD-XXXXXXXX");

        return new OrderNumber(trimmed);
    }

    public static OrderNumber Generate()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        return new OrderNumber($"ORD-{date}-{random}");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(OrderNumber number) => number.Value;
}