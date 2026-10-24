using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Services;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IBrandRepository _brandRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITypeRepository _typeRepository;

    public CreateProductHandler(
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

    public async Task<int> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productName = ProductName.Create(request.Name);
        var price = Money.Rub(request.Price);

        var product = new Product(
            productName,
            price,
            request.BrandId,
            request.CategoryId,
            request.TypeId,
            request.Description,
            request.StockQuantity);

        await CatalogReferenceValidator.EnsureExistAsync(
            _brandRepository, _categoryRepository, _typeRepository,
            request.BrandId, request.CategoryId, request.TypeId,
            cancellationToken);

        await _productRepository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}
