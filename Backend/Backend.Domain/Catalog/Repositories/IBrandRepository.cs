using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Catalog.Repositories;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Brand?> GetByNameAsync(string name, CancellationToken ct = default);

    Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(string name, CancellationToken ct = default);

    Task<bool> HasProductsAsync(int brandId, CancellationToken ct = default);

    Task AddAsync(Brand brand, CancellationToken ct = default);

    Task UpdateAsync(Brand brand, CancellationToken ct = default);

    Task DeleteAsync(Brand brand, CancellationToken ct = default);
}