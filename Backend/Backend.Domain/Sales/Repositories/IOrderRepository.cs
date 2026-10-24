using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Shared;

namespace Backend.Domain.Sales.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken ct = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);

    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);

    /// <summary>Заказы постранично (новые сверху), опционально — только в указанном статусе.</summary>
    Task<PagedResult<Order>> GetPagedAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default);

    Task AddAsync(Order order, CancellationToken ct = default);

    /// <summary>
    /// Атомарно сохраняет заказ и очищает корзину его владельца (одна транзакция БД).
    /// В ту же транзакцию попадают изменения остатков товаров, загруженных
    /// в рамках текущего запроса (резерв склада под заказ).
    /// </summary>
    Task AddAndClearCartAsync(Order order, CancellationToken ct = default);

    Task UpdateAsync(Order order, CancellationToken ct = default);

    Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);

    Task<decimal> SumRevenueAsync(CancellationToken ct = default);
}