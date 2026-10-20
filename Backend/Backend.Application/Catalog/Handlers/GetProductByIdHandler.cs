using MediatR;
using Backend.Application.Catalog.Queries;
using Backend.Application.DTOs;
using Backend.Domain.Catalog.Repositories;
using Backend.Application.Catalog.Mappers;

namespace Backend.Application.Catalog.Handlers;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            return null;

        return ProductMapper.MapToDto(product);   // используем общий маппер
    }
}