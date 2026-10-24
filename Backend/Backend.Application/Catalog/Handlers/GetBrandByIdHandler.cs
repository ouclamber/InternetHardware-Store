using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetBrandByIdHandler : IRequestHandler<GetBrandByIdQuery, BrandDto?>
{
    private readonly IBrandRepository _brandRepository;

    public GetBrandByIdHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<BrandDto?> Handle(
        GetBrandByIdQuery request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);

        if (brand == null)
            return null;

        return BrandMapper.MapToDto(brand);
    }
}