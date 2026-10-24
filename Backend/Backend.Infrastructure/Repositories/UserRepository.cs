using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Services;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache? _cache;

    /// <param name="cache">
    /// Кеш меток безопасности (см. <see cref="UserSessionValidator"/>). При изменении или удалении
    /// пользователя запись сбрасывается, чтобы отзыв токенов срабатывал сразу.
    /// </param>
    public UserRepository(ApplicationDbContext context, IMemoryCache? cache = null)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByUserNameAsync(UserName userName, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName, ct);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        // Приводим строку к VO
        var nameVo = UserName.Create(userName);
        return await GetByUserNameAsync(nameVo, ct);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .OrderBy(u => u.Id)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(UserName userName, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.UserName == userName, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.AttachForUpdate(user);
        await _context.SaveChangesAsync(ct);
        _cache?.Remove(UserSessionValidator.CacheKey(user.Id));
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
        _cache?.Remove(UserSessionValidator.CacheKey(user.Id));
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Users.CountAsync(ct);
    }

    public async Task<int> CountByRoleAsync(Role role, CancellationToken ct = default)
    {
        // Role — enum, конвертируется в string через HasConversion
        return await _context.Users.CountAsync(u => u.Role == role, ct);
    }

    public async Task<IReadOnlyDictionary<int, string>> GetUserNamesAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken ct = default)
    {
        if (ids == null || ids.Count == 0)
            return new Dictionary<int, string>();

        var idArray = ids.Distinct().ToArray();

        var rows = await _context.Users
            .AsNoTracking()
            .Where(u => idArray.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id, r => r.UserName.Value);
    }

    public async Task<string?> GetSecurityStampAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.SecurityStamp)
            .FirstOrDefaultAsync(ct);
    }
}
