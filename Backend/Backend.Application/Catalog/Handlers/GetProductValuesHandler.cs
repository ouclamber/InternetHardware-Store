using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class GetProductValuesHandler 
    : IRequestHandler<GetProductValuesQuery, IReadOnlyList<ProductAttributeValueDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductValuesHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyList<ProductAttributeValueDto>> Handle(
        GetProductValuesQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        return product.Values
            .Select(v => new ProductAttributeValueDto
            {
                Id = v.Id,
                ProductId = v.ProductId,
                AttributeId = v.AttributeId,
                AttributeName = v.ProductAttributes?.Name ?? "Неизвестно",
                Group = v.ProductAttributes?.AttributeGroup,
                Unit = v.ProductAttributes?.Unit,
                Value = v.Value
            })
            .ToList();
    }
}