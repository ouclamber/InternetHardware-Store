using Backend.Application.Sales.Handlers;
using Backend.Application.Sales.Queries;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Sales.Handlers;

public class GetUserOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _repo = new();
    private readonly Mock<IEncryptionService> _encryption = new();
    private readonly GetUserOrdersHandler _handler;

    public GetUserOrdersHandlerTests()
    {
        _handler = new GetUserOrdersHandler(_repo.Object, _encryption.Object);
    }

    private static Order CreateOrder(int userId, decimal price = 50000)
    {
        var items = new[]
        {
            (ProductId: 1, ProductName: "Item", Quantity: 1, UnitPrice: Money.Rub(price))
        };
        var address = Address.Create("ул. Ленина, д. 1", "Москва", "123456");
        return new Order(userId, address, items);
    }

    [Fact]
    public async Task Handle_ReturnsAllUserOrders()
    {
        var orders = new List<Order>
        {
            CreateOrder(userId: 1, price: 50000),
            CreateOrder(userId: 1, price: 1000),
            CreateOrder(userId: 1, price: 2500)
        };
        _repo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(orders);

        var result = await _handler.Handle(new GetUserOrdersQuery(1), default);

        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(1);
        result.Select(o => o.TotalAmount)
              .Should().BeEquivalentTo(new[] { 50000m, 1000m, 2500m });
    }

    [Fact]
    public async Task Handle_WhenNoOrders_ReturnsEmptyList()
    {
        _repo.Setup(r => r.GetByUserIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Order>());

        var result = await _handler.Handle(new GetUserOrdersQuery(999), default);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsEachOrderToDto()
    {
        var orders = new List<Order> { CreateOrder(userId: 7) };
        _repo.Setup(r => r.GetByUserIdAsync(7, It.IsAny<CancellationToken>()))
             .ReturnsAsync(orders);

        var result = await _handler.Handle(new GetUserOrdersQuery(7), default);

        var dto = result.Single();
        dto.UserId.Should().Be(7);
        dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
        dto.Status.Should().Be("pending");
        dto.StatusText.Should().NotBeNullOrWhiteSpace();
        dto.City.Should().Be("Москва");
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductName.Should().Be("Item");
    }
}