using MediatR;
using Backend.Application.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public int ProductId { get; set; }

    public GetProductByIdQuery(int productId)
    {
        ProductId = productId;
    }
}