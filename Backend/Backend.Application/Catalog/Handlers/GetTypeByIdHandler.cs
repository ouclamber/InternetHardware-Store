using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetTypeByIdHandler : IRequestHandler<GetTypeByIdQuery, ProductTypeDto?>
{
    private readonly ITypeRepository _typeRepository;

    public GetTypeByIdHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<ProductTypeDto?> Handle(
        GetTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var type = await _typeRepository.GetByIdAsync(request.TypeId, cancellationToken);

        if (type == null)
            return null;

        return ProductTypeMapper.MapToDto(type);
    }
}