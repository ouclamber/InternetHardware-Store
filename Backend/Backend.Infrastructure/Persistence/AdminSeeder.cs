using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;

namespace Backend.Infrastructure.Persistence;

/// <summary>
/// Создаёт первого администратора из конфигурации (Admin:UserName, Admin:Password).
/// Это единственный способ получить роль Admin «с нуля»: через регистрацию в API
/// администратором стать нельзя.
/// </summary>
public static class AdminSeeder
{
    public const int MinPasswordLength = 8;

    /// <returns>true, если администратор был создан или повышен.</returns>
    public static async Task<bool> EnsureAdminAsync(
        IUserRepository users,
        IPasswordHasher hasher,
        IConfiguration configuration,
        ILogger logger,
        CancellationToken ct = default)
    {
        var userName = configuration["Admin:UserName"];
        var password = configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation(
                "Admin:UserName/Admin:Password не заданы — создание администратора пропущено");
            return false;
        }

        if (password.Length < MinPasswordLength)
            throw new InvalidOperationException(
                $"Admin:Password должен содержать не менее {MinPasswordLength} символов");

        if (await users.CountByRoleAsync(Role.Admin, ct) > 0)
        {
            logger.LogInformation("Администратор уже существует — пропускаем");
            return false;
        }

        var name = UserName.Create(userName);

        var existing = await users.GetByUserNameAsync(name, ct);
        if (existing is not null)
        {
            existing.PromoteToAdmin();
            await users.UpdateAsync(existing, ct);
            logger.LogWarning(
                "Существующий пользователь {UserName} повышен до администратора", userName);
            return true;
        }

        var admin = new User(name, hasher.Hash(password), Role.Admin);
        await users.AddAsync(admin, ct);

        logger.LogInformation("Создан администратор {UserName}", userName);
        return true;
    }
}
