using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Симметричное шифрование строк алгоритмом AES-256-GCM.
///
/// GCM — это шифрование с аутентификацией: при подмене или повреждении данных
/// расшифровка выбросит исключение, а не вернёт мусор.
/// Для каждого вызова Encrypt генерируется новый случайный nonce, поэтому одинаковый
/// текст шифруется в разные строки.
///
/// Формат результата: Base64( nonce[12] | tag[16] | ciphertext ).
/// Ключ: 32 байта в Base64 (например, openssl rand -base64 32), параметр Encryption:Key.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int KeySize = 32;
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"];

        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException(
                "Encryption:Key не задан. Сгенерируйте ключ: openssl rand -base64 32");

        try
        {
            _key = Convert.FromBase64String(keyBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Encryption:Key должен быть строкой Base64 (32 байта)", ex);
        }

        if (_key.Length != KeySize)
            throw new InvalidOperationException(
                $"Encryption:Key должен содержать {KeySize} байта (после декодирования Base64), " +
                $"сейчас {_key.Length}");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var result = new byte[NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, result, NonceSize + TagSize, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <exception cref="CryptographicException">
    /// Данные повреждены, подделаны или зашифрованы другим ключом.
    /// </exception>
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        byte[] data;
        try
        {
            data = Convert.FromBase64String(cipherText);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Шифртекст имеет неверный формат", ex);
        }

        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Шифртекст слишком короткий");

        var nonce = data.AsSpan(0, NonceSize);
        var tag = data.AsSpan(NonceSize, TagSize);
        var cipherBytes = data.AsSpan(NonceSize + TagSize);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string? EncryptIfNotEmpty(string? plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return null;

        return Encrypt(plainText);
    }

    public string? DecryptIfNotEmpty(string? cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return null;

        return Decrypt(cipherText);
    }
}
