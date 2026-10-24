using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductValuesQuery : IRequest<IReadOnlyList<ProductAttributeValueDto>>
{
    public int ProductId { get; set; }

    public GetProductValuesQuery(int productId)
    {
        ProductId = productId;
    }
}