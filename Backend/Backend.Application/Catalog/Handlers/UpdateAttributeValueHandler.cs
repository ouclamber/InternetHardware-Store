using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateAttributeValueHandler : IRequestHandler<UpdateAttributeValueCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateAttributeValueHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        UpdateAttributeValueCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        // Product сам найдёт значение и обновит
        product.UpdateAttributeValue(request.AttributeId, request.NewValue);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}