namespace Backend.Domain.Shared;

/// <summary>
/// Единица работы: фиксирует все изменения отслеживаемых агрегатов одной транзакцией.
/// Используется в сценариях, затрагивающих несколько агрегатов
/// (например, отмена заказа + возврат остатков товаров на склад).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
