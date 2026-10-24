using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Repositories;

public enum ProductSort
{
    /// <summary>По умолчанию — в порядке добавления.</summary>
    Default = 0,
    PriceAsc = 1,
    PriceDesc = 2,
    NameAsc = 3,
    NameDesc = 4,
    Newest = 5
}

/// <summary>
/// Параметры выборки каталога: фильтры, сортировка, страница.
/// Все фильтры необязательны и комбинируются через AND.
/// </summary>
public sealed class ProductQuery
{
    public const int MaxSearchLength = 100;

    public int? CategoryId { get; init; }
    public int? TypeId { get; init; }
    public IReadOnlyCollection<int>? BrandIds { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Search { get; init; }
    public bool InStockOnly { get; init; }
    public bool IncludeInactive { get; init; }
    public ProductSort Sort { get; init; } = ProductSort.Default;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = Paging.DefaultPageSize;

    /// <summary>
    /// Проверить и привести параметры к допустимым значениям.
    /// </summary>
    public ProductQuery Normalize()
    {
        if (MinPrice is < 0 || MaxPrice is < 0)
            throw new DomainException("Цена в фильтре не может быть отрицательной");

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            throw new DomainException("Минимальная цена больше максимальной");

        var search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
        if (search is { Length: > MaxSearchLength })
            search = search[..MaxSearchLength];

        var (page, pageSize) = Paging.Normalize(Page, PageSize);

        var brands = BrandIds?.Where(id => id > 0).Distinct().ToArray();

        return new ProductQuery
        {
            CategoryId = CategoryId is > 0 ? CategoryId : null,
            TypeId = TypeId is > 0 ? TypeId : null,
            BrandIds = brands is { Length: > 0 } ? brands : null,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice,
            Search = search,
            InStockOnly = InStockOnly,
            IncludeInactive = IncludeInactive,
            Sort = Enum.IsDefined(Sort) ? Sort : ProductSort.Default,
            Page = page,
            PageSize = pageSize
        };
    }
}
