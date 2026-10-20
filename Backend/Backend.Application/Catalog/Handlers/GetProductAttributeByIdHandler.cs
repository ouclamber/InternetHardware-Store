using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetProductAttributeByIdHandler 
    : IRequestHandler<GetProductAttributeByIdQuery, ProductAttributeDto?>
{
    private readonly IProductAttributeRepository _attributeRepository;

    public GetProductAttributeByIdHandler(IProductAttributeRepository attributeRepository)
    {
        _attributeRepository = attributeRepository;
    }

    public async Task<ProductAttributeDto?> Handle(
        GetProductAttributeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var attribute = await _attributeRepository.GetByIdAsync(request.AttributeId, cancellationToken);

        return attribute == null ? null : ProductAttributeMapper.MapToDto(attribute);
    }
}