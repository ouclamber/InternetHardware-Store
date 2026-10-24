using MediatR;
using Backend.Application.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>
{
    public int Limit { get; set; } = 0;
}