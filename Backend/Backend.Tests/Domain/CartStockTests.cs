using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Shared;
using Backend.Tests.TestHelpers;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class CartStockTests
{
    [Fact]
    public void AddItem_MoreThanStock_Throws()
    {
        var cart = new Cart(1);
        var product = TestData.Product(stock: 2);

        var act = () => cart.AddItem(product, 3);

        act.Should().Throw<DomainException>().WithMessage("*доступно 2*");
        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void AddItem_RepeatedAdds_CannotExceedStockInTotal()
    {
        var cart = new Cart(1);
        var product = TestData.Product(stock: 3);
        cart.AddItem(product, 2);

        var act = () => cart.AddItem(product, 2);   // итого было бы 4 > 3

        act.Should().Throw<DomainException>();
        cart.GetItem(product.Id)!.Quantity.Value.Should().Be(2);
    }

    [Fact]
    public void AddItem_OutOfStock_Throws()
    {
        var cart = new Cart(1);

        var act = () => cart.AddItem(TestData.Product(stock: 0), 1);

        act.Should().Throw<DomainException>().WithMessage("*нет в наличии*");
    }

    [Fact]
    public void AddItem_InactiveProduct_Throws()
    {
        var cart = new Cart(1);

        var act = () => cart.AddItem(TestData.Product(stock: 5, active: false), 1);

        act.Should().Throw<DomainException>().WithMessage("*недоступен*");
    }

    [Fact]
    public void AddItem_LimitOfUniqueItems_Enforced()
    {
        var cart = new Cart(1);
        for (var i = 1; i <= Cart.MaxUniqueItems; i++)
            cart.AddItem(TestData.Product(id: i, stock: 5), 1);

        var act = () => cart.AddItem(TestData.Product(id: 999, stock: 5), 1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateItemQuantity_MoreThanStock_Throws()
    {
        var cart = new Cart(1);
        var product = TestData.Product(stock: 3);
        cart.AddItem(product, 1);

        var act = () => cart.UpdateItemQuantity(product.Id, 4);

        act.Should().Throw<DomainException>();
        cart.GetItem(product.Id)!.Quantity.Value.Should().Be(1);
    }

    [Fact]
    public void UpdateItemQuantity_RefreshesUnitPriceFromProduct()
    {
        var cart = new Cart(1);
        var product = TestData.Product(price: 1000, stock: 10);
        cart.AddItem(product, 1);

        product.UpdatePrice(Money.Rub(800));
        cart.UpdateItemQuantity(product.Id, 2);

        var item = cart.GetItem(product.Id)!;
        item.UnitPrice.Amount.Should().Be(800);
        item.TotalPrice.Amount.Should().Be(1600);
    }
}
