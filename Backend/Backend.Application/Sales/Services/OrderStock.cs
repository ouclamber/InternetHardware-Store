using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Sales.OrderAggregate;

namespace Backend.Application.Sales.Services;

/// <summary>
/// Возврат на склад товаров отменённого заказа.
/// </summary>
internal static class OrderStock
{
    public static async Task ReleaseAsync(
        Order order,
        IProductRepository productRepository,
        CancellationToken ct)
    {
        var toRelease = order.TakeStockToRelease();
        if (toRelease.Count == 0)
            return;

        var products = await productRepository.GetByIdsAsync(
            toRelease.Select(x => x.ProductId).ToList(), ct);

        var byId = products.ToDictionary(p => p.Id);

        foreach (var (productId, quantity) in toRelease)
        {
            // Товар мог быть удалён физически — тогда возвращать некуда
            if (byId.TryGetValue(productId, out var product))
                product.ReleaseStock(quantity);
        }
    }
}
