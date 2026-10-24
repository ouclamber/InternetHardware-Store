using Backend.Domain.Sales.ValueObjects;
using Backend.Infrastructure.Persistence.Converters;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Backend.Tests.Persistence;

public class CustomerContactConverterTests
{
    private static EncryptionService CreateEncryption(string? key = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = key ?? Convert.ToBase64String(Enumerable.Range(1, 32).Select(i => (byte)i).ToArray())
            })
            .Build();
        return new EncryptionService(config);
    }

    private static readonly CustomerContact Contact =
        CustomerContact.Create("Анна", "Смирнова", "anna@example.com", "+79990001122");

    [Fact]
    public void Serialize_WithEncryption_DoesNotContainPlainData()
    {
        var stored = CustomerContactConverter.Serialize(Contact, CreateEncryption());

        stored.Should().NotContain("anna@example.com");
        stored.Should().NotContain("Смирнова");
    }

    [Fact]
    public void RoundTrip_WithEncryption()
    {
        var encryption = CreateEncryption();

        var restored = CustomerContactConverter.Deserialize(
            CustomerContactConverter.Serialize(Contact, encryption), encryption);

        restored.Should().Be(Contact);
    }

    [Fact]
    public void RoundTrip_WithoutEncryption_StoresJson()
    {
        var stored = CustomerContactConverter.Serialize(Contact, null);

        stored.Should().StartWith("{");
        CustomerContactConverter.Deserialize(stored, null).Should().Be(Contact);
    }

    [Fact]
    public void Deserialize_LegacyPlainJson_WithEncryptionEnabled_Works()
    {
        var plain = CustomerContactConverter.Serialize(Contact, null);

        CustomerContactConverter.Deserialize(plain, CreateEncryption()).Should().Be(Contact);
    }

    [Fact]
    public void Deserialize_WrongKey_ReturnsPlaceholder_InsteadOfCrashing()
    {
        var stored = CustomerContactConverter.Serialize(Contact, CreateEncryption());
        var otherKey = Convert.ToBase64String(Enumerable.Range(100, 32).Select(i => (byte)i).ToArray());

        var restored = CustomerContactConverter.Deserialize(stored, CreateEncryption(otherKey));

        restored.FirstName.Should().Contain("не удалось");
        restored.Email.Should().BeEmpty();
    }
}
