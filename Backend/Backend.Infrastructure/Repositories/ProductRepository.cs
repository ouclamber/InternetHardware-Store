using Microsoft.EntityFrameworkCore;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Товар со всеми деталями. Загружается с отслеживанием: обработчики команд
    /// меняют его и сохраняют через UpdateAsync.
    /// </summary>
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .AsTracking()
            .AsSplitQuery()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Type)
            .Include(p => p.Images)
            .Include(p => p.Values)
                .ThenInclude(v => v.ProductAttributes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Count == 0)
            return Array.Empty<Product>();

        var idArray = ids.Distinct().ToArray();

        return await _context.Products
            .AsTracking()
            .Where(p => idArray.Contains(p.Id))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default)
    {
        return await ActiveWithListIncludes()
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(int take, CancellationToken ct = default)
    {
        if (take <= 0)
            return await GetActiveAsync(ct);

        return await ActiveWithListIncludes()
            .OrderBy(p => p.Id)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQuery query, CancellationToken ct = default)
    {
        var q = query.Normalize();

        var source = FilterBySqlPredicates(q);

        if (!q.IncludeInactive)
            source = source.Where(p => p.IsActive);

        if (q.CategoryId.HasValue)
        {
            var categoryId = q.CategoryId.Value;
            source = source.Where(p => p.CategoryId == categoryId);
        }

        if (q.TypeId.HasValue)
        {
            var typeId = q.TypeId.Value;
            source = source.Where(p => p.TypeId == typeId);
        }

        if (q.BrandIds is { Count: > 0 })
        {
            var brandIds = q.BrandIds.ToArray();
            source = source.Where(p => brandIds.Contains(p.BrandId));
        }

        if (q.InStockOnly)
            source = source.Where(p => p.StockQuantity > 0);

        var total = await source.CountAsync(ct);
        if (total == 0)
            return PagedResult<Product>.Empty(q.Page, q.PageSize);

        var items = await ApplySort(source, q.Sort)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Type)
            .Include(p => p.Images)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(ct);

        return new PagedResult<Product>(items, total, q.Page, q.PageSize);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByBrandAsync(int brandId, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.BrandId == brandId && p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<Product>();

        // Спецсимволы LIKE экранируются: запрос "100%" ищет именно "100%", а не всё подряд
        var pattern = ToLikePattern(query.Trim());

        return await _context.Products
            .FromSqlRaw(
                @"SELECT * FROM ""Products""
                WHERE ""IsActive"" = true
                    AND (""Name"" ILIKE {0} OR ""Description"" ILIKE {0})",
                pattern)
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .OrderBy(p => p.Id)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.AttachForUpdate(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(ct);
    }

    public async Task<int> CountActiveAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive, ct);
    }

    public async Task<int> CountOutOfStockAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.IsActive && p.StockQuantity == 0, ct);
    }

    // ============================================================
    // === Helpers ===
    // ============================================================

    private IQueryable<Product> ActiveWithListIncludes() =>
        _context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.IsActive)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Type)
            .Include(p => p.Images);

    /// <summary>
    /// Фильтры по цене и тексту. Цена хранится через value converter (Money → decimal),
    /// поэтому сравнения "больше/меньше" выполняются параметризованным SQL,
    /// а остальные фильтры дописываются LINQ поверх него (EF оборачивает SQL в подзапрос).
    /// </summary>
    private IQueryable<Product> FilterBySqlPredicates(ProductQuery q)
    {
        if (q.MinPrice is null && q.MaxPrice is null && q.Search is null)
            return _context.Products;

        var conditions = new List<string>();
        var parameters = new List<object>();

        if (q.MinPrice.HasValue)
        {
            conditions.Add($@"""Price"" >= {{{parameters.Count}}}");
            parameters.Add(q.MinPrice.Value);
        }

        if (q.MaxPrice.HasValue)
        {
            conditions.Add($@"""Price"" <= {{{parameters.Count}}}");
            parameters.Add(q.MaxPrice.Value);
        }

        if (q.Search != null)
        {
            var index = parameters.Count;
            conditions.Add($@"(""Name"" ILIKE {{{index}}} OR ""Description"" ILIKE {{{index}}})");
            parameters.Add(ToLikePattern(q.Search));
        }

        var sql = @"SELECT * FROM ""Products"" WHERE " + string.Join(" AND ", conditions);

        return _context.Products.FromSqlRaw(sql, parameters.ToArray());
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> source, ProductSort sort) => sort switch
    {
        ProductSort.PriceAsc => source.OrderBy(p => p.Price).ThenBy(p => p.Id),
        ProductSort.PriceDesc => source.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
        ProductSort.NameAsc => source.OrderBy(p => p.Name).ThenBy(p => p.Id),
        ProductSort.NameDesc => source.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
        ProductSort.Newest => source.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id),
        _ => source.OrderBy(p => p.Id)
    };

    /// <summary>Экранирование \, % и _ (в PostgreSQL обратный слэш — escape-символ LIKE по умолчанию).</summary>
    public static string ToLikePattern(string term)
    {
        var escaped = term
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");

        return $"%{escaped}%";
    }
}
