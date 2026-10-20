using Microsoft.EntityFrameworkCore;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class TypeRepository : ITypeRepository
{
    private readonly ApplicationDbContext _context;

    public TypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductType?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Types
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<ProductType?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Types
            .FirstOrDefaultAsync(t => t.Name == name, ct);
    }

    public async Task<IReadOnlyList<ProductType>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Types
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Types
            .AnyAsync(t => t.Name == name, ct);
    }

    public async Task<bool> HasProductsAsync(int typeId, CancellationToken ct = default)
    {
        return await _context.Products
            .AnyAsync(p => p.TypeId == typeId, ct);
    }

    public async Task AddAsync(ProductType type, CancellationToken ct = default)
    {
        await _context.Types.AddAsync(type, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ProductType type, CancellationToken ct = default)
    {
        _context.Types.Update(type);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ProductType type, CancellationToken ct = default)
    {
        _context.Types.Remove(type);
        await _context.SaveChangesAsync(ct);
    }
}