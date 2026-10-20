using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.ValueObjects;

public sealed class ProductName : ValueObject
{
    public const int MaxLength = 200;

    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название товара не может быть пустым");

        var trimmed = name.Trim();

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Название товара не может быть длиннее {MaxLength} символов");

        return new ProductName(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductName name) => name.Value;
}