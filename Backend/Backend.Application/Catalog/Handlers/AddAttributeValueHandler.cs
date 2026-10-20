using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class AddAttributeValueHandler : IRequestHandler<AddAttributeValueCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductAttributeRepository _attributeRepository;

    public AddAttributeValueHandler(
        IProductRepository productRepository,
        IProductAttributeRepository attributeRepository)
    {
        _productRepository = productRepository;
        _attributeRepository = attributeRepository;
    }

    public async Task Handle(
        AddAttributeValueCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        var attribute = await _attributeRepository.GetByIdAsync(request.AttributeId, cancellationToken);
        if (attribute == null)
            throw new DomainException($"Атрибут с Id={request.AttributeId} не найден");

        // Product сам проверит: не добавлен ли уже
        product.AddAttributeValue(request.AttributeId, request.Value);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}