namespace Backend.Domain.Shared;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string? EncryptIfNotEmpty(string? plainText);
    string? DecryptIfNotEmpty(string? cipherText);
}