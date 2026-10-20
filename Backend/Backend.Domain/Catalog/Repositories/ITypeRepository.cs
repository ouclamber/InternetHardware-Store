using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Catalog.Repositories;

public interface ITypeRepository
{
    Task<ProductType?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductType?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<ProductType>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
    Task<bool> HasProductsAsync(int typeId, CancellationToken ct = default);

    Task AddAsync(ProductType type, CancellationToken ct = default);
    Task UpdateAsync(ProductType type, CancellationToken ct = default);
    Task DeleteAsync(ProductType type, CancellationToken ct = default);
}