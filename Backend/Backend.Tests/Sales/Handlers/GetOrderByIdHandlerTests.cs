using Backend.Application.Sales.Handlers;
using Backend.Application.Sales.Queries;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Sales.Handlers;

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _handler = new GetOrderByIdHandler(_repo.Object);
    }

    private static Order CreateOrder(int userId = 1)
    {
        var items = new[]
        {
            (ProductId: 1, ProductName: "MacBook", Quantity: 2, UnitPrice: Money.Rub(50000)),
            (ProductId: 2, ProductName: "Mouse",   Quantity: 1, UnitPrice: Money.Rub(1500))
        };
        var address = Address.Create("ул. Ленина, д. 1", "Москва", "123456");
        return new Order(userId, address, items);
    }

    [Fact]
    public async Task Handle_AsOwner_ReturnsOrderDto()
    {
        var order = CreateOrder(userId: 1);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdQuery(1, 1, false),
            default);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(1);
        result.TotalAmount.Should().Be(101500);
        result.TotalQuantity.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.City.Should().Be("Москва");
        result.Address.Should().Be("ул. Ленина, д. 1");
        result.PostalCode.Should().Be("123456");
        result.Status.Should().Be("pending");
        result.StatusText.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_AsAdmin_CanViewOtherUsersOrder()
    {
        var order = CreateOrder(userId: 42);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdQuery(1, 1, true),
            default);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(42);
    }

    [Fact]
    public async Task Handle_WhenOrderMissing_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Order?)null);

        var result = await _handler.Handle(
            new GetOrderByIdQuery(999, 1, false),
            default);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NotOwnerNotAdmin_Throws()
    {
        var order = CreateOrder(userId: 42);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(order);

        Func<Task> act = () => _handler.Handle(
            new GetOrderByIdQuery(1, 999, false),
            default);

        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*доступ*");
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        var order = CreateOrder(userId: 1);
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(order);

        var result = await _handler.Handle(
            new GetOrderByIdQuery(1, 1, false),
            default);

        result!.OrderNumber.Should().NotBeNullOrWhiteSpace();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Items[0].ProductName.Should().Be("MacBook");
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(50000);
        result.Items[0].TotalPrice.Should().Be(100000);
    }
}