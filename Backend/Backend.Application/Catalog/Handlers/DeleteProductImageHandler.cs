using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeleteProductImageHandler : IRequestHandler<DeleteProductImageCommand>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductImageHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        DeleteProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        // Если удаляем главное — Product сам назначит новое главное
        product.RemoveImage(request.ImageId);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}