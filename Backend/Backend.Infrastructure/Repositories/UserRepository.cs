using Microsoft.EntityFrameworkCore;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
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
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
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
}