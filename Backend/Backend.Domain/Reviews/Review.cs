using Backend.Domain.Shared;
using Backend.Domain.Reviews.ValueObjects;

namespace Backend.Domain.Reviews;

/// <summary>
/// Отзыв о товаре — Aggregate Root.
/// 
/// Бизнес-правила:
/// - Рейтинг 1-5 (через VO Rating)
/// - Комментарий 10-2000 символов (через VO Comment)
/// - Один пользователь — ОДИН отзыв на товар (проверяется в репозитории)
/// - Автор может изменить свой отзыв (rating + comment)
/// - Админ может одобрить/отклонить (если нужна модерация)
/// - Отзыв создаётся СРАЗУ одобренным (autoApprove: true по умолчанию)
/// 
/// ⚠️ При edit — статус НЕ сбрасывается в Pending. 
/// Если в будущем нужна модерация — добавить в Edit блок сброса статуса.
/// </summary>
public class Review : Entity, IAggregateRoot
{
    public int UserId { get; private set; }
    public int ProductId { get; private set; }

    public Rating Rating { get; private set; } = null!;
    public Comment Comment { get; private set; } = null!;

    public ReviewStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ModeratedAt { get; private set; }

    /// <summary>
    /// Id админа, который одобрил/отклонил (null = не модерирован).
    /// </summary>
    public int? ModeratedByUserId { get; private set; }

    // Навигационные свойства (для EF Core)
    public Users.User? User { get; private set; }
    public Catalog.Entities.Product? Product { get; private set; }

    // Для EF Core
    protected Review() { }

    /// <summary>
    /// Создать новый отзыв.
    /// 
    /// autoApprove = true (по умолчанию) — отзыв сразу Approved, без модерации.
    /// autoApprove = false — статус Pending, ждёт модерации.
    /// </summary>
    public Review(
        int userId,
        int productId,
        Rating rating,
        Comment comment,
        bool autoApprove = true)
    {
        if (userId <= 0)
            throw new DomainException("Id пользователя обязателен");

        if (productId <= 0)
            throw new DomainException("Id товара обязателен");

        if (rating == null)
            throw new DomainException("Рейтинг обязателен");

        if (comment == null)
            throw new DomainException("Комментарий обязателен");

        UserId = userId;
        ProductId = productId;
        Rating = rating;
        Comment = comment;
        Status = autoApprove ? ReviewStatus.Approved : ReviewStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        if (autoApprove)
            ModeratedAt = DateTime.UtcNow;
    }

    // ============================================================
    // === Бизнес-методы ===
    // ============================================================

    /// <summary>
    /// Изменить содержимое отзыва (автор).
    /// 
    /// ✅ Статус сохраняется — если был Approved, остаётся Approved.
    /// Если Rejected — редактирование запрещено.
    /// </summary>
    public void Edit(Rating newRating, Comment newComment)
    {
        if (newRating == null)
            throw new DomainException("Рейтинг обязателен");

        if (newComment == null)
            throw new DomainException("Комментарий обязателен");

        if (Status == ReviewStatus.Rejected)
            throw new DomainException("Нельзя редактировать отклонённый отзыв");

        Rating = newRating;
        Comment = newComment;

        // ✅ Статус НЕ сбрасывается. Если нужна модерация — раскомментировать:
        // if (Status == ReviewStatus.Approved)
        // {
        //     Status = ReviewStatus.Pending;
        //     ModeratedAt = null;
        //     ModeratedByUserId = null;
        // }

        MarkAsUpdated();
    }

    /// <summary>
    /// Одобрить отзыв (админ).
    /// </summary>
    public void Approve(int adminUserId)
    {
        if (adminUserId <= 0)
            throw new DomainException("Id админа обязателен");

        if (Status == ReviewStatus.Approved)
            throw new DomainException("Отзыв уже одобрен");

        Status = ReviewStatus.Approved;
        ModeratedAt = DateTime.UtcNow;
        ModeratedByUserId = adminUserId;
        MarkAsUpdated();
    }

    /// <summary>
    /// Отклонить отзыв (админ).
    /// </summary>
    public void Reject(int adminUserId)
    {
        if (adminUserId <= 0)
            throw new DomainException("Id админа обязателен");

        if (Status == ReviewStatus.Rejected)
            throw new DomainException("Отзыв уже отклонён");

        Status = ReviewStatus.Rejected;
        ModeratedAt = DateTime.UtcNow;
        ModeratedByUserId = adminUserId;
        MarkAsUpdated();
    }

    // ============================================================
    // === Проверки ===
    // ============================================================

    public bool IsApproved => Status == ReviewStatus.Approved;
    public bool IsPending => Status == ReviewStatus.Pending;
    public bool IsRejected => Status == ReviewStatus.Rejected;

    /// <summary>
    /// Отзыв может быть отредактирован автором.
    /// </summary>
    public bool CanBeEditedByAuthor => Status != ReviewStatus.Rejected;

    // ============================================================
    // === Private ===
    // ============================================================

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}