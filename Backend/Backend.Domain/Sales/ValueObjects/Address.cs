using Backend.Domain.Shared;
using System.Text.Json.Serialization;

namespace Backend.Domain.Sales.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? PostalCode { get; }

    [JsonConstructor]
    private Address() { }

    private Address(string street, string city, string? postalCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
    }

    public static Address Create(string street, string city, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Улица обязательна");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("Город обязателен");

        var trimmedStreet = street.Trim();
        var trimmedCity = city.Trim();
        var trimmedPostal = postalCode?.Trim();

        if (trimmedStreet.Length > 200)
            throw new DomainException("Улица не может быть длиннее 200 символов");

        if (trimmedCity.Length > 100)
            throw new DomainException("Город не может быть длиннее 100 символов");

        if (trimmedPostal != null && trimmedPostal.Length > 20)
            throw new DomainException("Почтовый индекс не может быть длиннее 20 символов");

        return new Address(trimmedStreet, trimmedCity, trimmedPostal);
    }

    public string ToDisplayString()
    {
        var parts = new[] { City, Street, PostalCode }
            .Where(p => !string.IsNullOrWhiteSpace(p));

        return string.Join(", ", parts);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode ?? string.Empty;
    }

    public override string ToString() => ToDisplayString();
}