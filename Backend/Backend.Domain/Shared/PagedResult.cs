namespace Backend.Domain.Shared;

/// <summary>
/// Страница результатов + общее количество записей.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items ?? Array.Empty<T>();
        TotalCount = Math.Max(0, totalCount);
        Page = Math.Max(1, page);
        PageSize = Math.Max(1, pageSize);
    }

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20) => new(Array.Empty<T>(), 0, page, pageSize);

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), TotalCount, Page, PageSize);
}

/// <summary>
/// Нормализация параметров постраничного вывода.
/// </summary>
public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = MaxPageSize)
    {
        var p = page < 1 ? 1 : page;
        var size = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, maxPageSize);
        return (p, size);
    }
}
