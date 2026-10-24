using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class SetProductStockHandler : IRequestHandler<SetProductStockCommand>
{
    private readonly IProductRepository _productRepository;

    public SetProductStockHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(SetProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Товар с Id={request.ProductId} не найден");

        product.SetStock(request.Quantity);

        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}
