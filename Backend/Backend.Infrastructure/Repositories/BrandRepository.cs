using Microsoft.EntityFrameworkCore;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly ApplicationDbContext _context;

    public BrandRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Brand?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Brand?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Brands
            .FirstOrDefaultAsync(b => b.Name == name, ct);
    }

    public async Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Brands
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Brands
            .AnyAsync(b => b.Name == name, ct);
    }

    public async Task<bool> HasProductsAsync(int brandId, CancellationToken ct = default)
    {
        return await _context.Products
            .AnyAsync(p => p.BrandId == brandId, ct);
    }

    public async Task AddAsync(Brand brand, CancellationToken ct = default)
    {
        await _context.Brands.AddAsync(brand, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Brand brand, CancellationToken ct = default)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Brand brand, CancellationToken ct = default)
    {
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync(ct);
    }
}