using MediatR;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Application.DTOs;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class GetProductsPagedHandler : IRequestHandler<GetProductsPagedQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsPagedHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductDto>> Handle(
        GetProductsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = new ProductQuery
        {
            CategoryId = request.CategoryId,
            TypeId = request.TypeId,
            BrandIds = request.BrandIds,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Search = request.Search,
            InStockOnly = request.InStockOnly,
            IncludeInactive = false,   // покупателям — только активные товары
            Sort = ParseSort(request.Sort),
            Page = request.Page,
            PageSize = request.PageSize
        }.Normalize();

        var page = await _productRepository.GetPagedAsync(query, cancellationToken);

        return page.Map(ProductMapper.MapToDto);
    }

    public static ProductSort ParseSort(string? sort) => sort?.Trim().ToLowerInvariant() switch
    {
        null or "" or "default" or "relevance" => ProductSort.Default,
        "price_asc" => ProductSort.PriceAsc,
        "price_desc" => ProductSort.PriceDesc,
        "name_asc" => ProductSort.NameAsc,
        "name_desc" => ProductSort.NameDesc,
        "newest" => ProductSort.Newest,
        _ => throw new DomainException($"Неизвестная сортировка: {sort}")
    };
}
