using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderHandler(
        ICartRepository cartRepository,
        IOrderRepository orderRepository)
    {
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
    }

    public async Task<int> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null || cart.IsEmpty)
            throw new DomainException("Корзина пуста");

        // Валидация входных данных — до любых изменений состояния
        var address = Address.Create(
            request.Address,
            request.City,
            request.PostalCode);

        var contact = CustomerContact.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        // Раньше позиции без загруженного товара молча выкидывались из заказа,
        // а деактивированные товары можно было купить. Теперь — явная ошибка.
        foreach (var item in cart.Items)
        {
            if (item.Product == null)
                throw new DomainException($"Товар с Id={item.ProductId} больше не существует — удалите его из корзины");

            item.Product.EnsureCanBePurchased(item.Quantity.Value);
        }

        var items = cart.Items
            .Select(i => (
                ProductId: i.ProductId,
                ProductName: i.Product!.Name.Value,
                Quantity: i.Quantity.Value,
                UnitPrice: i.Product.Price))   // цена — актуальная на момент оформления
            .ToList();

        var order = new Order(
            userId: request.UserId,
            shippingAddress: address,
            items: items,
            deliveryMethod: request.DeliveryMethod,
            paymentMethod: request.PaymentMethod,
            comment: request.Comment,
            contact: contact);

        // Списываем остатки. Поле StockQuantity — токен конкуренции: если параллельный
        // заказ успел купить тот же товар, SaveChanges завершится конфликтом (409),
        // и склад не уйдёт в минус.
        foreach (var item in cart.Items)
            item.Product!.ReserveStock(item.Quantity.Value);

        order.MarkStockReserved();

        // Заказ, очистка корзины и списание остатков сохраняются одной транзакцией.
        await _orderRepository.AddAndClearCartAsync(order, cancellationToken);

        return order.Id;
    }
}
