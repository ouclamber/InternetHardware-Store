using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class AddProductImageHandler : IRequestHandler<AddProductImageCommand, int>
{
    private readonly IProductRepository _productRepository;

    public AddProductImageHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<int> Handle(
        AddProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        // Вся логика (max 10, single main) — внутри Product.AddImage
        product.AddImage(request.ImageUrl, request.AltText, request.IsMain);

        await _productRepository.UpdateAsync(product, cancellationToken);

        // Id последнего добавленного изображения
        var lastImage = product.Images.LastOrDefault();
        return lastImage?.Id ?? 0;
    }
}