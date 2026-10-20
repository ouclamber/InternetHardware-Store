using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetTypesHandler : IRequestHandler<GetTypesQuery, IReadOnlyList<ProductTypeDto>>
{
    private readonly ITypeRepository _typeRepository;

    public GetTypesHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<IReadOnlyList<ProductTypeDto>> Handle(
        GetTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await _typeRepository.GetAllAsync(cancellationToken);

        return types.Select(ProductTypeMapper.MapToDto).ToList();
    }
}