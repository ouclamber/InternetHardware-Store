namespace Backend.Domain.Reviews;

public enum ReviewStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public static class ReviewStatusExtensions
{
    public static string ToCode(this ReviewStatus status) => status switch
    {
        ReviewStatus.Pending => "pending",
        ReviewStatus.Approved => "approved",
        ReviewStatus.Rejected => "rejected",
        _ => "unknown"
    };

    public static string ToRussianString(this ReviewStatus status) => status switch
    {
        ReviewStatus.Pending => "Ожидает модерации",
        ReviewStatus.Approved => "Одобрен",
        ReviewStatus.Rejected => "Отклонён",
        _ => "Неизвестно"
    };

    public static ReviewStatus FromCode(string code) => code?.ToLowerInvariant() switch
    {
        "pending" => ReviewStatus.Pending,
        "approved" => ReviewStatus.Approved,
        "rejected" => ReviewStatus.Rejected,
        _ => throw new ArgumentException($"Неизвестный статус отзыва: {code}", nameof(code))
    };
}