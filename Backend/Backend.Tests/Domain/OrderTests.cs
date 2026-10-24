using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class OrderTests
{
    private static Address CreateAddress() =>
        Address.Create("ул. Ленина, д. 1, кв. 101", "Москва", "123456");

    private static Order CreateValidOrder()
    {
        var items = new[]
        {
            (ProductId: 1, ProductName: "MacBook", Quantity: 2, UnitPrice: Money.Rub(50000)),
            (ProductId: 2, ProductName: "Mouse",   Quantity: 1, UnitPrice: Money.Rub(1500))
        };
        return new Order(userId: 1, CreateAddress(), items);
    }

    [Fact]
    public void Constructor_CreatesOrderWithCorrectStatusAndTotal()
    {
        var order = CreateValidOrder();

        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().HaveCount(2);
        order.TotalAmount.Amount.Should().Be(101500); // 2*50000 + 1*1500
        order.UserId.Should().Be(1);
        order.OrderNumber.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidUserId_Throws(int userId)
    {
        Action act = () => new Order(userId, CreateAddress(),
            new[] { (1, "X", 1, Money.Rub(100)) });

        act.Should().Throw<DomainException>().WithMessage("*пользователя*");
    }

    [Fact]
    public void Constructor_WithNullAddress_Throws()
    {
        Action act = () => new Order(1, null!,
            new[] { (1, "X", 1, Money.Rub(100)) });

        act.Should().Throw<DomainException>().WithMessage("*Адрес*");
    }

    [Fact]
    public void Constructor_WithNullItems_Throws()
    {
        Action act = () => new Order(1, CreateAddress(), null!);

        act.Should().Throw<DomainException>().WithMessage("*товаров*");
    }

    [Fact]
    public void Constructor_WithEmptyItems_Throws()
    {
        Action act = () => new Order(1, CreateAddress(),
            Enumerable.Empty<(int, string, int, Money)>());

        act.Should().Throw<DomainException>().WithMessage("*пустым*");
    }

    [Fact]
    public void MarkAsPaid_FromPending_SetsPaidAndTimestamp()
    {
        var order = CreateValidOrder();

        order.MarkAsPaid();

        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsPaid_WhenNotPending_Throws()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();

        Action act = () => order.MarkAsPaid();

        act.Should().Throw<DomainException>().WithMessage("*Ожидает оплаты*");
    }

    [Fact]
    public void Ship_FromPaid_Succeeds()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
    }

    [Fact]
    public void Ship_FromPending_Throws()
    {
        var order = CreateValidOrder();

        Action act = () => order.Ship();

        act.Should().Throw<DomainException>().WithMessage("*оплаченный*");
    }

    [Fact]
    public void Deliver_FromShipped_Succeeds()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();
        order.Ship();

        order.Deliver();

        order.Status.Should().Be(OrderStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public void Deliver_FromPaid_Throws()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();

        Action act = () => order.Deliver();

        act.Should().Throw<DomainException>().WithMessage("*отправленный*");
    }

    [Fact]
    public void Cancel_FromPending_Succeeds()
    {
        var order = CreateValidOrder();

        order.Cancel("Передумал");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        order.CustomerComment.Should().Contain("Передумал");
    }

    [Fact]
    public void Cancel_FromDelivered_Throws()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();

        Action act = () => order.Cancel();

        act.Should().Throw<DomainException>().WithMessage("*доставленный*");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var order = CreateValidOrder();
        order.Cancel();

        Action act = () => order.Cancel();

        act.Should().Throw<DomainException>().WithMessage("*уже отменён*");
    }

    [Fact]
    public void Cancel_FromShipped_Throws()
    {
        var order = CreateValidOrder();
        order.MarkAsPaid();
        order.Ship();

        Action act = () => order.Cancel();

        act.Should().Throw<DomainException>().WithMessage("*отправленный*");
    }

    [Fact]
    public void StatusFlags_ReflectCurrentStatus()
    {
        var order = CreateValidOrder();
        order.IsPending.Should().BeTrue();
        order.IsPaid.Should().BeFalse();
        order.CanBeCancelled.Should().BeTrue();

        order.MarkAsPaid();
        order.IsPaid.Should().BeTrue();
        order.CanBeCancelled.Should().BeTrue();

        order.Ship();
        order.IsShipped.Should().BeTrue();
        order.CanBeCancelled.Should().BeFalse();
    }

    [Fact]
    public void TotalQuantity_SumsItemQuantities()
    {
        var order = CreateValidOrder();
        order.TotalQuantity.Should().Be(3); // 2 + 1
    }

    [Fact]
    public void SetShippingAddressFromStorage_UpdatesAddress()
    {
        var order = CreateValidOrder();
        var newAddress = Address.Create("Невский проспект, д. 2", "СПб", "654321");

        order.SetShippingAddressFromStorage(newAddress);

        order.ShippingAddress.City.Should().Be("СПб");
        order.ShippingAddress.Street.Should().Be("Невский проспект, д. 2");
    }

    [Fact]
    public void SetShippingAddressFromStorage_WithNull_Throws()
    {
        var order = CreateValidOrder();

        Action act = () => order.SetShippingAddressFromStorage(null!);

        act.Should().Throw<DomainException>().WithMessage("*null*");
    }
}