using MediatR;
using Backend.Application.Catalog.Queries;
using Backend.Application.DTOs;
using Backend.Domain.Catalog.Repositories;
using Backend.Application.Catalog.Mappers;

namespace Backend.Application.Catalog.Handlers;

public class SearchProductsHandler 
    : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Array.Empty<ProductDto>();

        var products = await _productRepository.SearchAsync(request.Query, cancellationToken);

        // Фильтрация на уровне Application (после загрузки)
        var filtered = products
            .Where(p => !request.MinPrice.HasValue || p.Price.Amount >= request.MinPrice.Value)
            .Where(p => !request.MaxPrice.HasValue || p.Price.Amount <= request.MaxPrice.Value)
            .Where(p => !request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value);

        return filtered
            .Select(ProductMapper.MapToDto)
            .ToList();
    }
}