using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class GetProductImagesHandler : IRequestHandler<GetProductImagesQuery, IReadOnlyList<ProductImageDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductImagesHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductImageDto>> Handle(
        GetProductImagesQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        return product.Images
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.Id)
            .Select(ProductImageMapper.MapToDto)
            .ToList();
    }
}