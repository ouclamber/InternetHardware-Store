using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeleteTypeHandler : IRequestHandler<DeleteTypeCommand>
{
    private readonly ITypeRepository _typeRepository;

    public DeleteTypeHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task Handle(
        DeleteTypeCommand request,
        CancellationToken cancellationToken)
    {
        var type = await _typeRepository.GetByIdAsync(request.TypeId, cancellationToken);

        if (type == null)
            throw new DomainException($"Тип с Id={request.TypeId} не найден");

        var hasProducts = await _typeRepository.HasProductsAsync(request.TypeId, cancellationToken);
        if (hasProducts)
            throw new DomainException(
                $"Нельзя удалить тип '{type.Name}': к нему привязаны товары");

        await _typeRepository.DeleteAsync(type, cancellationToken);
    }
}