using Microsoft.EntityFrameworkCore;
using Backend.Domain.Wishlist;
using Backend.Domain.Wishlist.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ApplicationDbContext _context;

    public WishlistRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WishlistItem>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Brand)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Images)
            .OrderByDescending(w => w.AddedAt)
            .ThenByDescending(w => w.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<int>> GetProductIdsAsync(int userId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => w.ProductId)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(int userId, int productId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .AsNoTracking()
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId, ct);
    }

    public async Task<int> CountByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.WishlistItems
            .AsNoTracking()
            .CountAsync(w => w.UserId == userId, ct);
    }

    public async Task AddAsync(WishlistItem item, CancellationToken ct = default)
    {
        await _context.WishlistItems.AddAsync(item, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveAsync(int userId, int productId, CancellationToken ct = default)
    {
        var item = await _context.WishlistItems
            .AsTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, ct);

        if (item == null)
            return false;

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
