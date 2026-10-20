using MediatR;
using Backend.Application.Catalog.Queries;
using Backend.Application.DTOs;
using Backend.Domain.Catalog.Repositories;
using Backend.Application.Catalog.Mappers;

namespace Backend.Application.Catalog.Handlers;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetActiveAsync(cancellationToken);

        var result = products
            .Select(ProductMapper.MapToDto)
            .AsEnumerable();

        if (request.Limit > 0)
            result = result.Take(request.Limit);

        return result.ToList();
    }
}