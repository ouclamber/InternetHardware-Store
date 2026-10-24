using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Первые <paramref name="take"/> активных товаров (лимит применяется в БД).</summary>
    Task<IReadOnlyList<Product>> GetActiveAsync(int take, CancellationToken ct = default);

    /// <summary>Каталог с фильтрами, сортировкой и пагинацией на стороне БД.</summary>
    Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default);

    /// <summary>Товары по списку Id (для изменения — сущности отслеживаются).</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default);

    Task AddAsync(Product product, CancellationToken ct = default);

    Task UpdateAsync(Product product, CancellationToken ct = default);

    Task DeleteAsync(Product product, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);

    Task<int> CountActiveAsync(CancellationToken ct = default);

    /// <summary>Активные товары, которых нет на складе.</summary>
    Task<int> CountOutOfStockAsync(CancellationToken ct = default);
}