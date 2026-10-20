using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
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
            request.Description);

        await _productRepository.AddAsync(product, cancellationToken);

        return product.Id;
    }
}