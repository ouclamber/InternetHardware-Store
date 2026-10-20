using Microsoft.EntityFrameworkCore;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .AsSplitQuery()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Type)
            .Include(p => p.Images)
            .Include(p => p.Values)
                .ThenInclude(v => v.ProductAttributes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Category)       // ← добавили: нужен для MapToDto
            .Include(p => p.Type)           // ← добавили
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.BrandId == brandId && p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<Product>();

        var pattern = $"%{query}%";

        return await _context.Products
            .FromSqlRaw(
                @"SELECT * FROM ""Products""
                WHERE ""IsActive"" = true
                    AND (""Name"" ILIKE {0} OR ""Description"" ILIKE {0})",
                pattern)
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(ct);
    }

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive, ct);
    }
}