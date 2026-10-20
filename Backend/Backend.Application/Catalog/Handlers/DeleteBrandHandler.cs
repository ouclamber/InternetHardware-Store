using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly IBrandRepository _brandRepository;

    public DeleteBrandHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task Handle(
        DeleteBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);

        if (brand == null)
            throw new DomainException($"Бренд с Id={request.BrandId} не найден");

        // Проверка: нельзя удалить, если есть товары
        var hasProducts = await _brandRepository.HasProductsAsync(request.BrandId, cancellationToken);
        if (hasProducts)
            throw new DomainException(
                $"Нельзя удалить бренд '{brand.Name}': к нему привязаны товары");

        await _brandRepository.DeleteAsync(brand, cancellationToken);
    }
}