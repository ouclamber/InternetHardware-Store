using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Persistence.Converters;

/// <summary>
/// Хранит контакты покупателя в одной колонке: JSON, зашифрованный AES-GCM.
/// Если сервис шифрования не передан (юнит-тесты с InMemory), JSON хранится как есть.
/// </summary>
public sealed class CustomerContactConverter : ValueConverter<CustomerContact, string>
{
    public CustomerContactConverter(IEncryptionService? encryption)
        : base(
            contact => Serialize(contact, encryption),
            value => Deserialize(value, encryption))
    {
    }

    private sealed record Payload(string F, string L, string E, string P);

    public static string Serialize(CustomerContact contact, IEncryptionService? encryption)
    {
        var json = JsonSerializer.Serialize(new Payload(contact.FirstName, contact.LastName, contact.Email, contact.Phone));
        return encryption == null ? json : encryption.Encrypt(json);
    }

    public static CustomerContact Deserialize(string value, IEncryptionService? encryption)
    {
        string json;

        // Значение, записанное без шифрования (например, до включения ключа), читаем как есть
        if (encryption == null || value.TrimStart().StartsWith('{'))
        {
            json = value;
        }
        else
        {
            try
            {
                json = encryption.Decrypt(value);
            }
            catch (CryptographicException)
            {
                // Данные зашифрованы другим ключом — не роняем загрузку заказа целиком
                return CustomerContact.FromStorage("[не удалось расшифровать]", string.Empty, string.Empty, string.Empty);
            }
        }

        var payload = JsonSerializer.Deserialize<Payload>(json)
                      ?? new Payload(string.Empty, string.Empty, string.Empty, string.Empty);

        return CustomerContact.FromStorage(payload.F, payload.L, payload.E, payload.P);
    }
}
