using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetBrandsQuery : IRequest<IReadOnlyList<BrandDto>>
{
    // Пока без параметров — все бренды
}