using Backend.Domain.Shared;

namespace Backend.Domain.Users.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    public const int MinLength = 20;   // BCrypt хеш ~60 символов
    public const int MaxLength = 200;

    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Хеш пароля не может быть пустым");

        var trimmed = hash.Trim();

        if (trimmed.Length < MinLength)
            throw new DomainException($"Хеш пароля слишком короткий (минимум {MinLength} символов)");

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Хеш пароля слишком длинный (максимум {MaxLength} символов)");

        return new PasswordHash(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => "[HASH]";   // Никогда не выводим хеш в логи!
}