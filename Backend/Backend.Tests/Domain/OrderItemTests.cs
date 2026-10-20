using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Constructor_ValidData_CreatesItem()
    {
        var item = new OrderItem(1, "MacBook", 2, Money.Rub(1000));

        item.ProductId.Should().Be(1);
        item.ProductName.Should().Be("MacBook");
        item.Quantity.Should().Be(2);
        item.UnitPrice.Amount.Should().Be(1000);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidProductId_ThrowsDomainException(int productId)
    {
        var act = () => new OrderItem(productId, "Test", 1, Money.Rub(1000));
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyProductName_ThrowsDomainException(string name)
    {
        var act = () => new OrderItem(1, name, 1, Money.Rub(1000));
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidQuantity_ThrowsDomainException(int quantity)
    {
        var act = () => new OrderItem(1, "Test", quantity, Money.Rub(1000));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_ZeroPrice_ThrowsDomainException()
    {
        var act = () => new OrderItem(1, "Test", 1, Money.Rub(0));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TrimsProductName()
    {
        var item = new OrderItem(1, "  MacBook  ", 1, Money.Rub(1000));
        item.ProductName.Should().Be("MacBook");
    }

    [Fact]
    public void TotalPrice_MultipliesUnitPriceByQuantity()
    {
        var item = new OrderItem(1, "Test", 3, Money.Rub(1000));
        item.TotalPrice.Amount.Should().Be(3000);
    }

    [Fact]
    public void SetOrderId_UpdatesOrderId()
    {
        var item = new OrderItem(1, "Test", 1, Money.Rub(1000));
        item.SetOrderId(42);
        item.OrderId.Should().Be(42);
    }
}