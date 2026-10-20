using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryDto?>
{
    public int CategoryId { get; set; }

    public GetCategoryByIdQuery(int categoryId)
    {
        CategoryId = categoryId;
    }
}