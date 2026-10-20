using System.Security.Cryptography;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Backend.Tests.Services;

public class EncryptionServiceTests
{
    // Тестовые ключи: 32 байта в Base64
    private static readonly string KeyA = Convert.ToBase64String(Enumerable.Repeat((byte)1, 32).ToArray());
    private static readonly string KeyB = Convert.ToBase64String(Enumerable.Repeat((byte)2, 32).ToArray());

    private static IConfiguration CreateConfig(string? key)
    {
        var dict = new Dictionary<string, string?>();
        if (key != null) dict["Encryption:Key"] = key;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private static EncryptionService CreateService(string? key = null) =>
        new(CreateConfig(key ?? KeyA));

    // === КОНСТРУКТОР / КЛЮЧ ===

    [Fact]
    public void Constructor_NoKey_Throws()
    {
        var act = () => new EncryptionService(CreateConfig(null));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_NotBase64Key_Throws()
    {
        var act = () => new EncryptionService(CreateConfig("это-не-base64!!!"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_WrongKeyLength_Throws()
    {
        var shortKey = Convert.ToBase64String(new byte[16]);
        var act = () => new EncryptionService(CreateConfig(shortKey));
        act.Should().Throw<InvalidOperationException>();
    }

    // === ШИФРОВАНИЕ ===

    [Fact]
    public void Encrypt_ValidText_ReturnsEncryptedString()
    {
        var encrypted = CreateService().Encrypt("Hello, World!");

        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe("Hello, World!");
    }

    [Fact]
    public void Encrypt_SameTextTwice_ReturnsDifferentStrings()
    {
        // Случайный nonce: одинаковый текст даёт разный шифртекст
        var service = CreateService();

        service.Encrypt("Hello").Should().NotBe(service.Encrypt("Hello"));
    }

    [Fact]
    public void Encrypt_EmptyText_ReturnsEmpty()
    {
        CreateService().Encrypt("").Should().Be("");
    }

    [Fact]
    public void Encrypt_NullText_ReturnsNull()
    {
        CreateService().Encrypt(null!).Should().BeNull();
    }

    // === РАСШИФРОВКА ===

    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("Привет, мир!")]
    [InlineData("a")]
    public void Decrypt_EncryptedText_ReturnsOriginal(string original)
    {
        var service = CreateService();

        service.Decrypt(service.Encrypt(original)).Should().Be(original);
    }

    [Fact]
    public void Decrypt_LongText_RoundTrips()
    {
        var service = CreateService();
        var original = new string('a', 1000);

        service.Decrypt(service.Encrypt(original)).Should().Be(original);
    }

    [Fact]
    public void Decrypt_EmptyText_ReturnsEmpty()
    {
        CreateService().Decrypt("").Should().Be("");
    }

    [Fact]
    public void Decrypt_InvalidBase64_Throws()
    {
        var act = () => CreateService().Decrypt("not-a-base64-string!!!");
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_TooShortData_Throws()
    {
        var tooShort = Convert.ToBase64String(new byte[5]);
        var act = () => CreateService().Decrypt(tooShort);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_TamperedData_Throws()
    {
        // GCM обнаруживает подмену данных
        var service = CreateService();
        var bytes = Convert.FromBase64String(service.Encrypt("secret"));
        bytes[^1] ^= 0xFF; // портим последний байт шифртекста

        var act = () => service.Decrypt(Convert.ToBase64String(bytes));

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WithDifferentKey_Throws()
    {
        var encrypted = CreateService(KeyA).Encrypt("secret");

        var act = () => CreateService(KeyB).Decrypt(encrypted);

        act.Should().Throw<CryptographicException>();
    }

    // === IfNotEmpty ===

    [Fact]
    public void EncryptIfNotEmpty_ValidText_ReturnsEncrypted()
    {
        var result = CreateService().EncryptIfNotEmpty("Hello");

        result.Should().NotBeNullOrEmpty();
        result.Should().NotBe("Hello");
    }

    [Fact]
    public void EncryptIfNotEmpty_EmptyText_ReturnsNull()
    {
        var service = CreateService();

        service.EncryptIfNotEmpty("").Should().BeNull();
        service.EncryptIfNotEmpty("   ").Should().BeNull();
        service.EncryptIfNotEmpty(null).Should().BeNull();
    }

    [Fact]
    public void DecryptIfNotEmpty_ValidText_ReturnsDecrypted()
    {
        var service = CreateService();

        service.DecryptIfNotEmpty(service.Encrypt("Hello")).Should().Be("Hello");
    }

    [Fact]
    public void DecryptIfNotEmpty_EmptyText_ReturnsNull()
    {
        var service = CreateService();

        service.DecryptIfNotEmpty("").Should().BeNull();
        service.DecryptIfNotEmpty(null).Should().BeNull();
    }
}
