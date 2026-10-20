using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeleteAttributeValueHandler : IRequestHandler<DeleteAttributeValueCommand>
{
    private readonly IProductRepository _productRepository;

    public DeleteAttributeValueHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        DeleteAttributeValueCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        product.RemoveAttributeValue(request.AttributeId);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}