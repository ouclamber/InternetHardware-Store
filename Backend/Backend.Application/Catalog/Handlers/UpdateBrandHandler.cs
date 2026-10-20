using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand>
{
    private readonly IBrandRepository _brandRepository;

    public UpdateBrandHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task Handle(
        UpdateBrandCommand request,
        CancellationToken cancellationToken)
    {
        var brand = await _brandRepository.GetByIdAsync(request.BrandId, cancellationToken);

        if (brand == null)
            throw new DomainException($"Бренд с Id={request.BrandId} не найден");

        // Проверка: не занято ли новое имя
        if (brand.Name != request.NewName)
        {
            var exists = await _brandRepository.ExistsAsync(request.NewName, cancellationToken);
            if (exists)
                throw new DomainException($"Бренд '{request.NewName}' уже существует");
        }

        // Вызвать бизнес-метод 
        brand.Rename(request.NewName);

        await _brandRepository.UpdateAsync(brand, cancellationToken);
    }
}