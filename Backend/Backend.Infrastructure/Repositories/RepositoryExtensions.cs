using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

internal static class RepositoryExtensions
{
    /// <summary>
    /// Подготовить агрегат к сохранению.
    ///
    /// Агрегаты для изменения загружаются с отслеживанием (AsTracking), и EF сам
    /// вычисляет изменения — в том числе удалённые из коллекций элементы (позиции корзины,
    /// изображения товара). Раньше репозитории вызывали Update() на отсоединённом графе:
    /// удаления из коллекций не сохранялись, а связанные сущности (товар, бренд, пользователь)
    /// перезаписывались устаревшими значениями.
    ///
    /// Update() остаётся только для объектов, созданных/загруженных вне текущего контекста.
    /// </summary>
    public static void AttachForUpdate<TEntity>(this DbContext context, TEntity entity)
        where TEntity : class
    {
        if (context.Entry(entity).State == EntityState.Detached)
            context.Update(entity);
    }
}
