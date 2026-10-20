namespace Backend.Domain.Shared;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Rub(decimal amount)
    {
        if (amount < 0)
            throw new DomainException($"Сумма не может быть отрицательной: {amount}");

        return new Money(amount, "RUB");
    }

    public static Money Of(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException($"Сумма не может быть отрицательной: {amount}");
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Валюта обязательна");

        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money Zero => new Money(0, "RUB");

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Нельзя сложить {Currency} и {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Нельзя вычесть {other.Currency} из {Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new DomainException($"Результат вычитания отрицательный: {result}");

        return new Money(result, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity < 0)
            throw new DomainException($"Количество не может быть отрицательным: {quantity}");

        return new Money(Amount * quantity, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Нельзя сравнить {Currency} и {other.Currency}");
        
        return Amount > other.Amount;
    }

    public bool IsZero => Amount == 0;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:N0} {Currency}";
    }
}