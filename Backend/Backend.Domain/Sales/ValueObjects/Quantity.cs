using Backend.Domain.Shared;

namespace Backend.Domain.Sales.ValueObjects;

public sealed class Quantity : ValueObject
{
    public const int MaxValue = 999;

    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Of(int value)
    {
        if (value <= 0)
            throw new DomainException($"Количество должно быть больше 0, получено: {value}");

        if (value > MaxValue)
            throw new DomainException($"Количество не может быть больше {MaxValue}, получено: {value}");

        return new Quantity(value);
    }

    public static Quantity One => new Quantity(1);

    public Quantity Add(int delta)
    {
        if (delta < 0)
            throw new DomainException("Для увеличения используйте положительное число");

        return Of(Value + delta);
    }

    public Quantity Subtract(int delta)
    {
        if (delta < 0)
            throw new DomainException("Для уменьшения используйте положительное число");

        return Of(Value - delta);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(Quantity q) => q.Value;
}