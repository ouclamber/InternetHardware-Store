using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>
{
    public bool RootOnly { get; set; }

    public GetCategoriesQuery(bool rootOnly = false)
    {
        RootOnly = rootOnly;
    }
}