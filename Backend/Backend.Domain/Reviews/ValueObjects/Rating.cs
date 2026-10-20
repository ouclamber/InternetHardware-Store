using Backend.Domain.Shared;

namespace Backend.Domain.Reviews.ValueObjects;

public sealed class Rating : ValueObject
{
    public const int MinValue = 1;
    public const int MaxValue = 5;

    public int Value { get; }

    private Rating(int value)
    {
        Value = value;
    }

    public static Rating Of(int value)
    {
        if (value < MinValue)
            throw new DomainException($"Рейтинг не может быть меньше {MinValue}, получено: {value}");

        if (value > MaxValue)
            throw new DomainException($"Рейтинг не может быть больше {MaxValue}, получено: {value}");

        return new Rating(value);
    }

    public static Rating One => new(1);
    public static Rating Two => new(2);
    public static Rating Three => new(3);
    public static Rating Four => new(4);
    public static Rating Five => new(5);

    public bool IsPositive => Value >= 4;

    public bool IsNegative => Value <= 2;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(Rating rating) => rating.Value;
}