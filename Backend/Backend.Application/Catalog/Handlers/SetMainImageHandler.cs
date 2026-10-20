using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class SetMainImageHandler : IRequestHandler<SetMainImageCommand>
{
    private readonly IProductRepository _productRepository;

    public SetMainImageHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        SetMainImageCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        // Бизнес-метод сам убирает IsMain у остальных
        product.MakeImageMain(request.ImageId);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}