using Microsoft.EntityFrameworkCore;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Carts
            .AsNoTracking()
            .AsSplitQuery()                    // убирает cartesian explosion
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Brand)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Carts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Brand)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.AddAsync(cart, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken ct = default)
    {
        _context.Carts.Update(cart);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Cart cart, CancellationToken ct = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ClearByUserIdAsync(int userId, CancellationToken ct = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart == null || !cart.Items.Any())
            return;

        // ⚠️ Явно удаляем items через контекст — EF гарантированно пометит их Deleted
        _context.CartItems.RemoveRange(cart.Items);
        cart.Clear();                     // чистим доменную коллекцию для консистентности
        await _context.SaveChangesAsync(ct);
    }
}