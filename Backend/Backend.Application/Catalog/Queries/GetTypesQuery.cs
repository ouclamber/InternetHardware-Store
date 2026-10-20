using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetTypesQuery : IRequest<IReadOnlyList<ProductTypeDto>>
{
}