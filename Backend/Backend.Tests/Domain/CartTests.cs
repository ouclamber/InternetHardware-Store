using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Shared;
using FluentAssertions;
using System.Reflection;

namespace Backend.Tests.Domain;

public class CartTests
{
    private static void SetId(Entity entity, int id)
    {
        var prop = typeof(Entity).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        prop?.SetValue(entity, id);
    }

    private static Product CreateProduct(int id, decimal price = 1000, bool active = true)
    {
        var product = new Product(
            ProductName.Create("Product " + id),
            Money.Rub(price),
            brandId: 1, categoryId: 1, typeId: 1);
        SetId(product, id);
        if (!active) product.Deactivate();
        return product;
    }

    [Fact]
    public void Constructor_CreatesEmptyCart()
    {
        var cart = new Cart(userId: 1);

        cart.UserId.Should().Be(1);
        cart.IsEmpty.Should().BeTrue();
        cart.TotalAmount.Amount.Should().Be(0);
        cart.TotalQuantity.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidUserId_Throws(int userId)
    {
        Action act = () => new Cart(userId);
        act.Should().Throw<DomainException>().WithMessage("*пользователя*");
    }

    [Fact]
    public void AddItem_AddsNewItem()
    {
        var cart = new Cart(1);
        var product = CreateProduct(1);

        cart.AddItem(product, quantity: 2);

        cart.Items.Should().HaveCount(1);
        cart.TotalQuantity.Should().Be(2);
        cart.TotalAmount.Amount.Should().Be(2000);
    }

    [Fact]
    public void AddItem_WithNullProduct_Throws()
    {
        var cart = new Cart(1);

        Action act = () => cart.AddItem(null!, 1);

        act.Should().Throw<DomainException>().WithMessage("*не указан*");
    }

    [Fact]
    public void AddItem_WithInactiveProduct_Throws()
    {
        var cart = new Cart(1);
        var product = CreateProduct(1, active: false);

        Action act = () => cart.AddItem(product, 1);

        act.Should().Throw<DomainException>().WithMessage("*недоступен*");
    }

    [Fact]
    public void AddItem_ExistingProduct_IncreasesQuantity()
    {
        var cart = new Cart(1);
        var product = CreateProduct(1);
        cart.AddItem(product, 1);

        cart.AddItem(product, 2);

        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Value.Should().Be(3);
    }

    [Fact]
    public void UpdateItemQuantity_UpdatesQuantity()
    {
        var cart = new Cart(1);
        var product = CreateProduct(1);
        cart.AddItem(product, 1);

        cart.UpdateItemQuantity(1, 5);

        cart.Items.First().Quantity.Value.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_WithZero_RemovesItem()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);

        cart.UpdateItemQuantity(1, 0);

        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void UpdateItemQuantity_WithNegative_RemovesItem()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);

        cart.UpdateItemQuantity(1, -5);

        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void UpdateItemQuantity_WithMissingProduct_Throws()
    {
        var cart = new Cart(1);

        Action act = () => cart.UpdateItemQuantity(999, 5);

        act.Should().Throw<DomainException>().WithMessage("*не найден*");
    }

    [Fact]
    public void RemoveItem_RemovesExisting()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);

        cart.RemoveItem(1);

        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void RemoveItem_WithMissing_Throws()
    {
        var cart = new Cart(1);

        Action act = () => cart.RemoveItem(999);

        act.Should().Throw<DomainException>().WithMessage("*не найден*");
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);
        cart.AddItem(CreateProduct(2), 1);

        cart.Clear();

        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void ContainsProduct_ReturnsTrueIfExists()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);

        cart.ContainsProduct(1).Should().BeTrue();
        cart.ContainsProduct(999).Should().BeFalse();
    }

    [Fact]
    public void GetItem_ReturnsItemOrNull()
    {
        var cart = new Cart(1);
        cart.AddItem(CreateProduct(1), 1);

        cart.GetItem(1).Should().NotBeNull();
        cart.GetItem(999).Should().BeNull();
    }
}