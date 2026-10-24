using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Services;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITypeRepository _typeRepository;

    public UpdateProductHandler(
        IProductRepository productRepository,
        IBrandRepository brandRepository,
        ICategoryRepository categoryRepository,
        ITypeRepository typeRepository)
    {
        _productRepository = productRepository;
        _brandRepository = brandRepository;
        _categoryRepository = categoryRepository;
        _typeRepository = typeRepository;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Товар с Id={request.ProductId} не найден");

        // Сначала валидация всех значений, затем изменения — объект не останется полуобновлённым
        var name = ProductName.Create(request.Name);
        var price = Money.Rub(request.Price);
        if (price.IsZero)
            throw new DomainException("Цена должна быть больше 0");

        await CatalogReferenceValidator.EnsureExistAsync(
            _brandRepository, _categoryRepository, _typeRepository,
            request.BrandId, request.CategoryId, request.TypeId,
            cancellationToken);

        product.Rename(name);
        product.UpdateDescription(request.Description);
        product.UpdatePrice(price);
        product.ChangeBrand(request.BrandId);
        product.ChangeCategory(request.CategoryId);
        product.ChangeType(request.TypeId);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}
