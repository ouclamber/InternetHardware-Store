using Backend.Domain.Sales.CartAggregate;

namespace Backend.Domain.Sales.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default);

    Task AddAsync(Cart cart, CancellationToken ct = default);

    Task UpdateAsync(Cart cart, CancellationToken ct = default);

    Task DeleteAsync(Cart cart, CancellationToken ct = default);

    Task ClearByUserIdAsync(int userId, CancellationToken ct = default);
}