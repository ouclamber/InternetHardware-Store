using Microsoft.EntityFrameworkCore;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Orders
            .AsTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var numberVo = OrderNumber.FromString(orderNumber);
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == numberVo, ct);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.Status == status)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (p, size) = Paging.Normalize(page, pageSize);

        var query = _context.Orders.AsNoTracking();
        if (status.HasValue)
        {
            var s = status.Value;
            query = query.Where(o => o.Status == s);
        }

        var total = await query.CountAsync(ct);
        if (total == 0)
            return PagedResult<Order>.Empty(p, size);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((p - 1) * size)
            .Take(size)
            .Include(o => o.Items)
            .AsSplitQuery()
            .ToListAsync(ct);

        return new PagedResult<Order>(items, total, p, size);
    }

    public async Task<IReadOnlyList<Order>> GetRecentAsync(int count = 10, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddAndClearCartAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);

        // AsTracking: корзина обычно уже загружена обработчиком в этом же контексте —
        // получаем тот же экземпляр, без конфликта идентичности
        var cart = await _context.Carts
            .AsTracking()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == order.UserId, ct);

        if (cart != null && cart.Items.Any())
        {
            _context.CartItems.RemoveRange(cart.Items);
            cart.Clear();
        }

        // Один SaveChanges = одна транзакция БД: заказ, очистка корзины и списание
        // остатков товаров (отслеживаются в этом же контексте) — либо всё, либо ничего.
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.AttachForUpdate(order);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Orders.CountAsync(ct);
    }

    public async Task<decimal> SumRevenueAsync(CancellationToken ct = default)
    {
        var connection = _context.Database.GetDbConnection();
        await using var command = connection.CreateCommand();

        command.CommandText = @"
            SELECT COALESCE(SUM(""TotalAmount""), 0)
            FROM ""Orders""
            WHERE ""Status"" != 'cancelled'";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        var result = await command.ExecuteScalarAsync(ct);

        return result == null || result == DBNull.Value
            ? 0m
            : Convert.ToDecimal(result);
    }
}