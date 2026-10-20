namespace Backend.Domain.Shared;

public interface IJwtService
{
    string GenerateToken(int userId, string userName, string role);
}