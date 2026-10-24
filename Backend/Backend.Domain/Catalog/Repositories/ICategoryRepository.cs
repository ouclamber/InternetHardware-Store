using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Catalog.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
    Task<bool> HasProductsAsync(int categoryId, CancellationToken ct = default);

    /// <summary>Количество активных товаров по категориям (одним GROUP BY-запросом).</summary>
    Task<IReadOnlyDictionary<int, int>> GetActiveProductCountsAsync(CancellationToken ct = default);
    Task<bool> HasSubCategoriesAsync(int categoryId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
}