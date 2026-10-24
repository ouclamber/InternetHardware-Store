using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Backend.Domain.Users.Repositories;

namespace Backend.Infrastructure.Services;

/// <summary>
/// Проверяет, что JWT выдан для актуальной «сессии» пользователя:
/// метка безопасности из токена должна совпадать с текущей меткой в БД.
/// Метка меняется при смене пароля и роли, а при удалении пользователя её нет вовсе —
/// так старые токены перестают работать сразу, а не через сутки.
/// </summary>
public interface IUserSessionValidator
{
    Task<bool> IsValidAsync(int userId, string? securityStamp, CancellationToken ct = default);
}

public sealed class UserSessionValidator : IUserSessionValidator
{
    /// <summary>
    /// Короткий кеш: проверка выполняется на каждый запрос с токеном, а ходить в БД каждый раз дорого.
    /// Репозиторий пользователей сбрасывает запись при изменении, так что задержка — только
    /// между разными экземплярами приложения.
    /// </summary>
    public static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private readonly IUserRepository _users;
    private readonly IMemoryCache _cache;

    public UserSessionValidator(IUserRepository users, IMemoryCache cache)
    {
        _users = users;
        _cache = cache;
    }

    public static string CacheKey(int userId) => $"user-security-stamp:{userId}";

    public async Task<bool> IsValidAsync(int userId, string? securityStamp, CancellationToken ct = default)
    {
        if (userId <= 0 || string.IsNullOrEmpty(securityStamp))
            return false;

        var current = await _cache.GetOrCreateAsync(CacheKey(userId), entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return _users.GetSecurityStampAsync(userId, ct);
        });

        if (string.IsNullOrEmpty(current))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(current),
            Encoding.UTF8.GetBytes(securityStamp));
    }
}
