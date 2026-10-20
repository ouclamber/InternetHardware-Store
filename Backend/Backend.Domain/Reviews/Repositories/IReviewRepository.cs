using Backend.Domain.Reviews;

namespace Backend.Domain.Reviews.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetByProductAsync(int productId, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetByUserAsync(int userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int userId, int productId, CancellationToken ct = default);

    Task<Review?> GetByUserAndProductAsync(int userId, int productId, CancellationToken ct = default);

    Task<int> GetCountByProductAsync(int productId, CancellationToken ct = default);

    Task<double> GetAverageRatingAsync(int productId, CancellationToken ct = default);

    Task<IReadOnlyList<Review>> GetPendingAsync(CancellationToken ct = default);

    Task AddAsync(Review review, CancellationToken ct = default);

    Task UpdateAsync(Review review, CancellationToken ct = default);

    Task DeleteAsync(Review review, CancellationToken ct = default);
}