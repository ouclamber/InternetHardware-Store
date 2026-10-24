using Backend.Domain.Users.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public PasswordHash Hash(string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            throw new ArgumentException("Пароль не может быть пустым", nameof(plainPassword));

        var hash = BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt(WorkFactor));
        return PasswordHash.FromHash(hash);
    }

    public bool Verify(string plainPassword, PasswordHash passwordHash)
    {
        if (string.IsNullOrEmpty(plainPassword))
            return false;

        if (passwordHash == null)
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash.Value);
        }
        catch
        {
            return false;
        }
    }
}