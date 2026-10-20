using Backend.Domain.Sales.OrderAggregate;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class OrderStatusTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, "pending")]
    [InlineData(OrderStatus.Paid, "paid")]
    [InlineData(OrderStatus.Shipped, "shipped")]
    [InlineData(OrderStatus.Delivered, "delivered")]
    [InlineData(OrderStatus.Cancelled, "cancelled")]
    public void ToCode_ReturnsExpected(OrderStatus status, string expected)
    {
        status.ToCode().Should().Be(expected);
    }

    [Theory]
    [InlineData("pending", OrderStatus.Pending)]
    [InlineData("paid", OrderStatus.Paid)]
    [InlineData("shipped", OrderStatus.Shipped)]
    [InlineData("delivered", OrderStatus.Delivered)]
    [InlineData("cancelled", OrderStatus.Cancelled)]
    [InlineData("PENDING", OrderStatus.Pending)]
    [InlineData("PAID", OrderStatus.Paid)]
    public void FromCode_ValidCode_ReturnsStatus(string code, OrderStatus expected)
    {
        OrderStatusExtensions.FromCode(code).Should().Be(expected);
    }

    [Fact]
    public void FromCode_InvalidCode_ThrowsArgumentException()
    {
        var act = () => OrderStatusExtensions.FromCode("invalid");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToRussianString_Pending_ReturnsRussian()
    {
        OrderStatus.Pending.ToRussianString().Should().Contain("Ожидает");
    }

    [Fact]
    public void ToRussianString_Paid_ReturnsRussian()
    {
        OrderStatus.Paid.ToRussianString().Should().Contain("Опла");
    }

    [Fact]
    public void ToRussianString_Delivered_ReturnsRussian()
    {
        OrderStatus.Delivered.ToRussianString().Should().Contain("Достав");
    }
}