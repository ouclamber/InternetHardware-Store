using Backend.Domain.Shared;

namespace Backend.Domain.Users.ValueObjects;

public sealed class UserName : ValueObject
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    public string Value { get; }

    private UserName(string value)
    {
        Value = value;
    }

    public static UserName Create(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new DomainException("Имя пользователя обязательно");

        var trimmed = userName.Trim();

        if (trimmed.Length < MinLength)
            throw new DomainException($"Имя пользователя должно быть не короче {MinLength} символов");

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Имя пользователя не может быть длиннее {MaxLength} символов");

        if (char.IsDigit(trimmed[0]))
            throw new DomainException("Имя пользователя не может начинаться с цифры");

        foreach (var ch in trimmed)
        {
            if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '-')
                throw new DomainException(
                    $"Имя пользователя может содержать только буквы, цифры, '_' и '-'. Недопустимый символ: '{ch}'");
        }

        return new UserName(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(UserName name) => name.Value;
}