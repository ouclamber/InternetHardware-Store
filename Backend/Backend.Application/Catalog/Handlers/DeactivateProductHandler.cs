using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeactivateProductHandler : IRequestHandler<DeactivateProductCommand>
{
    private readonly IProductRepository _productRepository;

    public DeactivateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        DeactivateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        product.Deactivate();

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}