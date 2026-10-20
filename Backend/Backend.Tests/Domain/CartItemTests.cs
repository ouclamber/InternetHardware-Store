using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using System.Reflection;

namespace Backend.Tests.Domain;

public class CartItemTests
{
    private static Product CreateProduct(int id = 1, decimal price = 1000)
    {
        var product = new Product(
            ProductName.Create("Test Product"),
            Money.Rub(price),
            brandId: 1,
            categoryId: 1,
            typeId: 1);

        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    [Fact]
    public void Constructor_ValidData_CreatesItem()
    {
        var product = CreateProduct(5, 500);
        var item = new CartItem(product, Quantity.Of(3));

        item.ProductId.Should().Be(5);
        item.Quantity.Value.Should().Be(3);
        item.UnitPrice.Amount.Should().Be(500);
    }

    [Fact]
    public void Constructor_NullProduct_ThrowsDomainException()
    {
        var act = () => new CartItem(null!, Quantity.Of(1));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_NullQuantity_ThrowsDomainException()
    {
        var act = () => new CartItem(CreateProduct(), null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TotalPrice_CalculatesCorrectly()
    {
        var item = new CartItem(CreateProduct(1, 1000), Quantity.Of(3));
        item.TotalPrice.Amount.Should().Be(3000);
    }

    [Fact]
    public void IncreaseQuantity_AddsDelta()
    {
        var item = new CartItem(CreateProduct(), Quantity.Of(2));
        item.IncreaseQuantity(3);
        item.Quantity.Value.Should().Be(5);
    }

    [Fact]
    public void SetQuantity_ReplacesQuantity()
    {
        var item = new CartItem(CreateProduct(), Quantity.Of(2));
        item.SetQuantity(Quantity.Of(10));
        item.Quantity.Value.Should().Be(10);
    }

    [Fact]
    public void UpdateUnitPrice_UpdatesPrice()
    {
        var item = new CartItem(CreateProduct(), Quantity.Of(1));
        item.UpdateUnitPrice(Money.Rub(2000));
        item.UnitPrice.Amount.Should().Be(2000);
    }

    [Fact]
    public void SetCartId_UpdatesCartId()
    {
        var item = new CartItem(CreateProduct(), Quantity.Of(1));
        item.SetCartId(42);
        item.CartId.Should().Be(42);
    }
}