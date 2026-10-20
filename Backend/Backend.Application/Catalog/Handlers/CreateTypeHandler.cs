using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateTypeHandler : IRequestHandler<CreateTypeCommand, int>
{
    private readonly ITypeRepository _typeRepository;

    public CreateTypeHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task<int> Handle(
        CreateTypeCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _typeRepository.ExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new DomainException($"Тип '{request.Name}' уже существует");

        var type = new ProductType(request.Name);

        await _typeRepository.AddAsync(type, cancellationToken);

        return type.Id;
    }
}