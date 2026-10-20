using Backend.Domain.Catalog.Entities;

namespace Backend.Domain.Catalog.Repositories;

public interface IProductAttributeRepository
{
    Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductAttribute?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<ProductAttribute>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistsAsync(string name, CancellationToken ct = default);
    Task<bool> HasValuesAsync(int attributeId, CancellationToken ct = default);

    Task AddAsync(ProductAttribute attribute, CancellationToken ct = default);
    Task UpdateAsync(ProductAttribute attribute, CancellationToken ct = default);
    Task DeleteAsync(ProductAttribute attribute, CancellationToken ct = default);
}