using Backend.Domain.Users.ValueObjects;

namespace Backend.Domain.Shared;

public interface IPasswordHasher
{
    PasswordHash Hash(string plainPassword);
    bool Verify(string plainPassword, PasswordHash passwordHash);
}