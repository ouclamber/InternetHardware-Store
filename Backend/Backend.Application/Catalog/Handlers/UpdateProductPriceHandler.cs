using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateProductPriceHandler : IRequestHandler<UpdateProductPriceCommand>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductPriceHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task Handle(
        UpdateProductPriceCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Загрузить товар
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

        // 2. Создать VO Money (валидация: цена > 0)
        var newPrice = Money.Rub(request.NewPrice);

        // 3. Вызвать бизнес-метод (не сеттер!)
        product.UpdatePrice(newPrice);

        // 4. Сохранить
        await _productRepository.UpdateAsync(product, cancellationToken);
    }
}