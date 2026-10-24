using System.Text.RegularExpressions;
using Backend.Domain.Shared;

namespace Backend.Domain.Sales.ValueObjects;

/// <summary>
/// Контактные данные покупателя, зафиксированные в заказе.
/// Это персональные данные — в БД хранятся в зашифрованном виде (AES-GCM).
/// </summary>
public sealed class CustomerContact : ValueObject
{
    public const int MaxNameLength = 100;
    public const int MaxEmailLength = 200;

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string Phone { get; }

    private CustomerContact(string firstName, string lastName, string email, string phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
    }

    public static CustomerContact Create(string firstName, string lastName, string email, string phone)
    {
        var first = RequireName(firstName, "Имя");
        var last = RequireName(lastName, "Фамилия");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email обязателен");

        var trimmedEmail = email.Trim();
        if (trimmedEmail.Length > MaxEmailLength || !EmailRegex.IsMatch(trimmedEmail))
            throw new DomainException("Некорректный email");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Телефон обязателен");

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 10 || digits.Length > 15)
            throw new DomainException("Некорректный номер телефона");

        var normalizedPhone = phone.TrimStart().StartsWith('+') ? "+" + digits : digits;

        return new CustomerContact(first, last, trimmedEmail.ToLowerInvariant(), normalizedPhone);
    }

    /// <summary>Восстановление из хранилища без повторной валидации.</summary>
    public static CustomerContact FromStorage(string firstName, string lastName, string email, string phone)
        => new(firstName ?? string.Empty, lastName ?? string.Empty, email ?? string.Empty, phone ?? string.Empty);

    public string FullName => $"{FirstName} {LastName}".Trim();

    private static string RequireName(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} обязательно");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxNameLength)
            throw new DomainException($"{field}: не длиннее {MaxNameLength} символов");

        return trimmed;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Email;
        yield return Phone;
    }

    // Персональные данные не должны попадать в логи
    public override string ToString() => "[CONTACT]";
}
