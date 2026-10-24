using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using Backend.Tests.TestHelpers;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ProductStockTests
{
    [Fact]
    public void Constructor_DefaultStock_IsZero_AndNotInStock()
    {
        var product = new Product(ProductName.Create("P"), Money.Rub(10), 1, 1, 1);

        product.StockQuantity.Should().Be(0);
        product.IsInStock.Should().BeFalse();
        product.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithStock_SetsStock()
    {
        var product = new Product(ProductName.Create("P"), Money.Rub(10), 1, 1, 1, stockQuantity: 7);

        product.StockQuantity.Should().Be(7);
        product.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NegativeStock_Throws()
    {
        var act = () => new Product(ProductName.Create("P"), Money.Rub(10), 1, 1, 1, stockQuantity: -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TooLongDescription_Throws()
    {
        var act = () => new Product(ProductName.Create("P"), Money.Rub(10), 1, 1, 1,
            new string('x', Product.MaxDescriptionLength + 1));

        act.Should().Throw<DomainException>().WithMessage("*Описание*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(Product.MaxStockQuantity)]
    public void SetStock_ValidValue_Sets(int quantity)
    {
        var product = TestData.Product(stock: 3);

        product.SetStock(quantity);

        product.StockQuantity.Should().Be(quantity);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(Product.MaxStockQuantity + 1)]
    public void SetStock_InvalidValue_Throws(int quantity)
    {
        var product = TestData.Product(stock: 3);

        var act = () => product.SetStock(quantity);

        act.Should().Throw<DomainException>();
        product.StockQuantity.Should().Be(3);
    }

    [Fact]
    public void ReserveStock_DecreasesStock()
    {
        var product = TestData.Product(stock: 5);

        product.ReserveStock(2);

        product.StockQuantity.Should().Be(3);
    }

    [Fact]
    public void ReserveStock_ExactlyAllStock_LeavesZero()
    {
        var product = TestData.Product(stock: 2);

        product.ReserveStock(2);

        product.StockQuantity.Should().Be(0);
        product.IsInStock.Should().BeFalse();
    }

    [Fact]
    public void ReserveStock_MoreThanAvailable_Throws_AndKeepsStock()
    {
        var product = TestData.Product(stock: 2);

        var act = () => product.ReserveStock(3);

        act.Should().Throw<DomainException>().WithMessage("*доступно 2*");
        product.StockQuantity.Should().Be(2);
    }

    [Fact]
    public void ReserveStock_OutOfStock_ThrowsNotInStock()
    {
        var product = TestData.Product(stock: 0);

        var act = () => product.ReserveStock(1);

        act.Should().Throw<DomainException>().WithMessage("*нет в наличии*");
    }

    [Fact]
    public void ReserveStock_InactiveProduct_Throws()
    {
        var product = TestData.Product(stock: 10, active: false);

        var act = () => product.ReserveStock(1);

        act.Should().Throw<DomainException>().WithMessage("*недоступен*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ReserveStock_NonPositive_Throws(int quantity)
    {
        var product = TestData.Product(stock: 10);

        var act = () => product.ReserveStock(quantity);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ReleaseStock_IncreasesStock()
    {
        var product = TestData.Product(stock: 1);

        product.ReleaseStock(4);

        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public void ReleaseStock_NonPositive_Throws()
    {
        var product = TestData.Product(stock: 1);

        var act = () => product.ReleaseStock(0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void EnsureCanBePurchased_DoesNotChangeStock()
    {
        var product = TestData.Product(stock: 5);

        product.EnsureCanBePurchased(5);

        product.StockQuantity.Should().Be(5);
    }
}

public class MoneyComparisonTests
{
    [Fact]
    public void CompareTo_SameCurrency_ComparesAmounts()
    {
        Money.Rub(10).CompareTo(Money.Rub(20)).Should().BeLessThan(0);
        Money.Rub(20).CompareTo(Money.Rub(10)).Should().BeGreaterThan(0);
        Money.Rub(10).CompareTo(Money.Rub(10)).Should().Be(0);
    }

    [Fact]
    public void CompareTo_Null_IsGreater()
    {
        Money.Rub(1).CompareTo(null).Should().BeGreaterThan(0);
    }

    [Fact]
    public void CompareTo_DifferentCurrency_Throws()
    {
        var act = () => Money.Rub(1).CompareTo(Money.Of(1, "USD"));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Sorting_UsesAmount()
    {
        var sorted = new[] { Money.Rub(30), Money.Rub(10), Money.Rub(20) }.OrderBy(m => m).Select(m => m.Amount).ToList();

        sorted.Should().Equal(10m, 20m, 30m);
    }

    [Fact]
    public void ProductName_CompareTo_IsCaseInsensitive()
    {
        ProductName.Create("apple").CompareTo(ProductName.Create("APPLE")).Should().Be(0);
        ProductName.Create("Apple").CompareTo(ProductName.Create("Banana")).Should().BeLessThan(0);
    }
}
