namespace Backend.Domain.Wishlist.Repositories;

public interface IWishlistRepository
{
    /// <summary>Избранное пользователя вместе с товарами (бренд, изображения), новые сверху.</summary>
    Task<IReadOnlyList<WishlistItem>> GetByUserAsync(int userId, CancellationToken ct = default);

    /// <summary>Только Id товаров — лёгкий запрос для отметок «в избранном» в каталоге.</summary>
    Task<IReadOnlyList<int>> GetProductIdsAsync(int userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int userId, int productId, CancellationToken ct = default);

    Task<int> CountByUserAsync(int userId, CancellationToken ct = default);

    Task AddAsync(WishlistItem item, CancellationToken ct = default);

    /// <returns>true, если запись была удалена.</returns>
    Task<bool> RemoveAsync(int userId, int productId, CancellationToken ct = default);
}
