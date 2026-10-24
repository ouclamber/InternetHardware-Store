namespace Backend.Domain.Shared;

public interface IJwtService
{
    /// <param name="securityStamp">
    /// Метка безопасности пользователя: попадает в токен и сверяется при каждом запросе,
    /// поэтому после смены пароля/роли старые токены становятся недействительными.
    /// </param>
    string GenerateToken(int userId, string userName, string role, string securityStamp);
}
