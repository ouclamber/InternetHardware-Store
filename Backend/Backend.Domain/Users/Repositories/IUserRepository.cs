using Backend.Domain.Users.ValueObjects;

namespace Backend.Domain.Users.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<User?> GetByUserNameAsync(UserName userName, CancellationToken ct = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(UserName userName, CancellationToken ct = default);

    Task AddAsync(User user, CancellationToken ct = default);

    Task UpdateAsync(User user, CancellationToken ct = default);

    Task DeleteAsync(User user, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);

    Task<int> CountByRoleAsync(Role role, CancellationToken ct = default);
}