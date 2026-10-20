using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateTypeHandler : IRequestHandler<UpdateTypeCommand>
{
    private readonly ITypeRepository _typeRepository;

    public UpdateTypeHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task Handle(
        UpdateTypeCommand request,
        CancellationToken cancellationToken)
    {
        var type = await _typeRepository.GetByIdAsync(request.TypeId, cancellationToken);

        if (type == null)
            throw new DomainException($"Тип с Id={request.TypeId} не найден");

        if (type.Name != request.NewName)
        {
            var exists = await _typeRepository.ExistsAsync(request.NewName, cancellationToken);
            if (exists)
                throw new DomainException($"Тип '{request.NewName}' уже существует");
        }

        type.Rename(request.NewName);

        await _typeRepository.UpdateAsync(type, cancellationToken);
    }
}