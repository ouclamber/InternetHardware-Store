using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductAttributeByIdQuery : IRequest<ProductAttributeDto?>
{
    public int AttributeId { get; set; }

    public GetProductAttributeByIdQuery(int attributeId)
    {
        AttributeId = attributeId;
    }
}