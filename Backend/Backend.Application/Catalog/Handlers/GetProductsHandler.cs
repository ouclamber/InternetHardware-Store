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
        // Лимит применяется в БД: раньше загружался весь каталог, а Take делался в памяти
        var products = request.Limit > 0
            ? await _productRepository.GetActiveAsync(request.Limit, cancellationToken)
            : await _productRepository.GetActiveAsync(cancellationToken);

        return products
            .Select(ProductMapper.MapToDto)
            .ToList();
    }
}