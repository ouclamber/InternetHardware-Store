using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Domain.Sales.CartAggregate;
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

        var address = Address.Create(
            request.Address,
            request.City,
            request.PostalCode);

        var items = cart.Items
            .Where(i => i.Product != null)
            .Select(i => (
                ProductId: i.ProductId,
                ProductName: i.Product!.Name.Value,
                Quantity: i.Quantity.Value,
                UnitPrice: i.Product.Price));

        var order = new Order(
            userId: request.UserId,
            shippingAddress: address,
            items: items,
            deliveryMethod: request.DeliveryMethod,
            paymentMethod: request.PaymentMethod,
            comment: request.Comment);

        // Заказ и очистка корзины сохраняются атомарно: нельзя получить
        // созданный заказ при непустой корзине (и дубль при повторном оформлении).
        await _orderRepository.AddAndClearCartAsync(order, cancellationToken);

        return order.Id;
    }
}