using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetProductAttributesHandler 
    : IRequestHandler<GetProductAttributesQuery, IReadOnlyList<ProductAttributeDto>>
{
    private readonly IProductAttributeRepository _attributeRepository;

    public GetProductAttributesHandler(IProductAttributeRepository attributeRepository)
    {
        _attributeRepository = attributeRepository;
    }

    public async Task<IReadOnlyList<ProductAttributeDto>> Handle(
        GetProductAttributesQuery request,
        CancellationToken cancellationToken)
    {
        var attributes = await _attributeRepository.GetAllAsync(cancellationToken);

        return attributes.Select(ProductAttributeMapper.MapToDto).ToList();
    }
}