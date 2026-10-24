using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetBrandsHandler : IRequestHandler<GetBrandsQuery, IReadOnlyList<BrandDto>>
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandsHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<IReadOnlyList<BrandDto>> Handle(
        GetBrandsQuery request,
        CancellationToken cancellationToken)
    {
        var brands = await _brandRepository.GetAllAsync(cancellationToken);

        return brands.Select(BrandMapper.MapToDto).ToList();
    }
}