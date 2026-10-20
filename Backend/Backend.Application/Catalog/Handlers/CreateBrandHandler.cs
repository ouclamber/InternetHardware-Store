using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, int>
{
    private readonly IBrandRepository _brandRepository;

    public CreateBrandHandler(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<int> Handle(
        CreateBrandCommand request,
        CancellationToken cancellationToken)
    {
        // Проверка уникальности
        var exists = await _brandRepository.ExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new DomainException($"Бренд '{request.Name}' уже существует");

        var brand = new Brand(request.Name);

        await _brandRepository.AddAsync(brand, cancellationToken);

        return brand.Id;
    }
}