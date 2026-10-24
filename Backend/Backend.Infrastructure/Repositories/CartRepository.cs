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

    /// <summary>
    /// Корзина пользователя с товарами. Загружается с отслеживанием: изменения позиций
    /// (в т.ч. удаление) EF вычисляет сам при SaveChanges.
    /// </summary>
    public async Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await CartWithProducts()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<Cart?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await CartWithProducts()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.AddAsync(cart, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Cart cart, CancellationToken ct = default)
    {
        _context.AttachForUpdate(cart);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Cart cart, CancellationToken ct = default)
    {
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ClearByUserIdAsync(int userId, CancellationToken ct = default)
    {
        // AsTracking: если корзина уже загружена в этом запросе, получим тот же экземпляр
        // (без конфликта идентичности при RemoveRange)
        var cart = await _context.Carts
            .AsTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart == null || !cart.Items.Any())
            return;

        _context.CartItems.RemoveRange(cart.Items);
        cart.Clear();
        await _context.SaveChangesAsync(ct);
    }

    private IQueryable<Cart> CartWithProducts() =>
        _context.Carts
            .AsTracking()
            .AsSplitQuery()                    // убирает cartesian explosion
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Brand)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images);
}
