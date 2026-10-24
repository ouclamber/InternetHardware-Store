namespace Backend.Application.Reviews;

/// <summary>
/// Настройки отзывов (секция конфигурации "Reviews").
/// </summary>
public class ReviewOptions
{
    public const string SectionName = "Reviews";

    /// <summary>
    /// true — новые и отредактированные отзывы попадают на модерацию (статус Pending)
    /// и не видны покупателям, пока администратор их не одобрит.
    /// false — отзывы публикуются сразу (поведение по умолчанию).
    /// </summary>
    public bool RequireModeration { get; set; }
}
