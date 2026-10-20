using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Handlers;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Sales.Handlers;

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _handler = new UpdateOrderStatusHandler(_repo.Object);
    }

    private static Order CreateOrder(OrderStatus initialStatus = OrderStatus.Pending)
    {
        var items = new[]
        {
            (ProductId: 1, ProductName: "Item", Quantity: 1, UnitPrice: Money.Rub(1000))
        };
        var address = Address.Create("ул. Ленина, д. 1", "Москва", "123456");
        var order = new Order(1, address, items);

        switch (initialStatus)
        {
            case OrderStatus.Pending: break;
            case OrderStatus.Paid: order.MarkAsPaid(); break;
            case OrderStatus.Shipped: order.MarkAsPaid(); order.Ship(); break;
            case OrderStatus.Delivered: order.MarkAsPaid(); order.Ship(); order.Deliver(); break;
            case OrderStatus.Cancelled: order.Cancel(); break;
        }
        return order;
    }

    private void SetupRepo(Order? order)
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(order);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_Paid_TransitionsToPaid()
    {
        var order = CreateOrder();
        SetupRepo(order);

        await _handler.Handle(new UpdateOrderStatusCommand(1, "paid", 99), default);

        order.Status.Should().Be(OrderStatus.Paid);
        _repo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Shipped_TransitionsToShipped()
    {
        var order = CreateOrder(OrderStatus.Paid);
        SetupRepo(order);

        await _handler.Handle(new UpdateOrderStatusCommand(1, "shipped", 99), default);

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public async Task Handle_Delivered_TransitionsToDelivered()
    {
        var order = CreateOrder(OrderStatus.Shipped);
        SetupRepo(order);

        await _handler.Handle(new UpdateOrderStatusCommand(1, "delivered", 99), default);

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public async Task Handle_Cancelled_TransitionsToCancelled()
    {
        var order = CreateOrder();
        SetupRepo(order);

        await _handler.Handle(new UpdateOrderStatusCommand(1, "cancelled", 42), default);

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CustomerComment.Should().Contain("42");
    }

    [Theory]
    [InlineData("PAID")]
    [InlineData("Paid")]
    [InlineData("paid")]
    public async Task Handle_StatusIsCaseInsensitive(string status)
    {
        var order = CreateOrder();
        SetupRepo(order);

        await _handler.Handle(new UpdateOrderStatusCommand(1, status, 99), default);

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task Handle_OrderMissing_Throws()
    {
        SetupRepo(null);

        Func<Task> act = () => _handler.Handle(
            new UpdateOrderStatusCommand(999, "paid", 99),
            default);

        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_UnknownStatus_Throws()
    {
        var order = CreateOrder();
        SetupRepo(order);

        Func<Task> act = () => _handler.Handle(
            new UpdateOrderStatusCommand(1, "foobar", 99),
            default);

        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*Недопустимый*");
    }

    [Fact]
    public async Task Handle_InvalidTransition_Throws()
    {
        var order = CreateOrder(); // Pending
        SetupRepo(order);

        // Нельзя "delivered" из Pending
        Func<Task> act = () => _handler.Handle(
            new UpdateOrderStatusCommand(1, "delivered", 99),
            default);

        await act.Should().ThrowAsync<DomainException>();
    }
}