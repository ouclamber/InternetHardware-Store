using Microsoft.EntityFrameworkCore;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.Repositories;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _context;

    public ReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Review>> GetByProductAsync(int productId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Review>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(int userId, int productId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId, ct);
    }

    public async Task<Review?> GetByUserAndProductAsync(int userId, int productId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId, ct);
    }

    public async Task<int> GetCountByProductAsync(int productId, CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .CountAsync(r => r.ProductId == productId && r.Status == ReviewStatus.Approved, ct);
    }

    public async Task<double> GetAverageRatingAsync(int productId, CancellationToken ct = default)
    {
        // Усреднение в SQL — не грузим все рейтинги в память
        var avg = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .AverageAsync(r => (double?)r.Rating.Value, ct);

        return avg ?? 0;
    }

    public async Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.Status == ReviewStatus.Pending)
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Review review, CancellationToken ct = default)
    {
        await _context.Reviews.AddAsync(review, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Review review, CancellationToken ct = default)
    {
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(ct);
    }
}