using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;

namespace Backend.Domain.Wishlist;

/// <summary>
/// Товар в списке избранного пользователя.
/// Пара (UserId, ProductId) уникальна — повторно добавить тот же товар нельзя.
/// </summary>
public class WishlistItem : Entity, IAggregateRoot
{
    /// <summary>Максимальный размер списка избранного одного пользователя.</summary>
    public const int MaxItemsPerUser = 100;

    public int UserId { get; private set; }
    public int ProductId { get; private set; }
    public DateTime AddedAt { get; private set; }

    public Product? Product { get; private set; }

    // Для EF Core
    protected WishlistItem() { }

    public WishlistItem(int userId, Product product)
    {
        if (userId <= 0)
            throw new DomainException("Id пользователя обязателен");

        if (product == null)
            throw new DomainException("Товар не указан");

        if (!product.IsActive)
            throw new DomainException($"Товар '{product.Name.Value}' недоступен");

        UserId = userId;
        ProductId = product.Id;
        Product = product;
        AddedAt = DateTime.UtcNow;
    }
}
