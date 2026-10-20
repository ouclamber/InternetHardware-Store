using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductImagesQuery : IRequest<IReadOnlyList<ProductImageDto>>
{
    public int ProductId { get; set; }

    public GetProductImagesQuery(int productId)
    {
        ProductId = productId;
    }
}