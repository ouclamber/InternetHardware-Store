using Microsoft.EntityFrameworkCore;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class ProductAttributeRepository : IProductAttributeRepository
{
    private readonly ApplicationDbContext _context;

    public ProductAttributeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductAttribute?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ProductAttributes
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<ProductAttribute?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.ProductAttributes
            .FirstOrDefaultAsync(a => a.Name == name, ct);
    }

    public async Task<IReadOnlyList<ProductAttribute>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ProductAttributes
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.ProductAttributes
            .AnyAsync(a => a.Name == name, ct);
    }

    public async Task<bool> HasValuesAsync(int attributeId, CancellationToken ct = default)
    {
        return await _context.ProductAttributeValues
            .AnyAsync(v => v.AttributeId == attributeId, ct);
    }

    public async Task AddAsync(ProductAttribute attribute, CancellationToken ct = default)
    {
        await _context.ProductAttributes.AddAsync(attribute, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProductAttribute attribute, CancellationToken ct = default)
    {
        _context.ProductAttributes.Update(attribute);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ProductAttribute attribute, CancellationToken ct = default)
    {
        _context.ProductAttributes.Remove(attribute);
        await _context.SaveChangesAsync(ct);
    }
}