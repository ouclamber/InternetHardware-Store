using MediatR;
using Backend.Application.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductsByCategoryQuery : IRequest<IReadOnlyList<ProductDto>>
{
    public int CategoryId { get; set; }

    public GetProductsByCategoryQuery(int categoryId)
    {
        CategoryId = categoryId;
    }
}