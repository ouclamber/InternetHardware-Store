using MediatR;
using Backend.Application.Catalog.Queries;
using Backend.Application.DTOs;
using Backend.Domain.Catalog.Repositories;
using Backend.Application.Catalog.Mappers;

namespace Backend.Application.Catalog.Handlers;

public class GetProductsByCategoryHandler 
    : IRequestHandler<GetProductsByCategoryQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetProductsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CategoryId <= 0)
            throw new ArgumentException("CategoryId обязателен");

        var products = await _productRepository.GetByCategoryAsync(request.CategoryId, cancellationToken);

        return products
            .Select(ProductMapper.MapToDto)
            .ToList();
    }
}