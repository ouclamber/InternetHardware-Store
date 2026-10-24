using MediatR;
using Backend.Application.DTOs;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Queries;

/// <summary>
/// Каталог с фильтрами, сортировкой и пагинацией на стороне сервера.
/// Раньше фронтенд скачивал всю категорию и фильтровал/листал её в браузере.
/// </summary>
public class GetProductsPagedQuery : IRequest<PagedResult<ProductDto>>
{
    public int? CategoryId { get; set; }
    public int? TypeId { get; set; }
    public IReadOnlyCollection<int>? BrandIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public bool InStockOnly { get; set; }

    /// <summary>
    /// Код сортировки: price_asc, price_desc, name_asc, name_desc, newest. Пусто — по умолчанию.
    /// </summary>
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = Paging.DefaultPageSize;
}
