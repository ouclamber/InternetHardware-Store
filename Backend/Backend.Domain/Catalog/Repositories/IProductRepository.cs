using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Catalog.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    Task UpdateAsync(Product product, CancellationToken ct = default);

    Task DeleteAsync(Product product, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);

    Task<int> CountActiveAsync(CancellationToken ct = default);
}