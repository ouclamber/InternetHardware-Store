using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class MoneyTests
{

    [Fact]
    public void Rub_ValidAmount_CreatesRubleMoney()
    {
        var money = Money.Rub(1000);
        money.Amount.Should().Be(1000);
        money.Currency.Should().Be("RUB");
    }

    [Fact]
    public void Rub_NegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Rub(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Of_ValidAmountAndCurrency_CreatesMoney()
    {
        var money = Money.Of(500, "usd");
        money.Amount.Should().Be(500);
        money.Currency.Should().Be("USD"); // ToUpperInvariant
    }

    [Fact]
    public void Of_EmptyCurrency_ThrowsDomainException()
    {
        var act = () => Money.Of(500, "");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Of_NegativeAmount_ThrowsDomainException()
    {
        var act = () => Money.Of(-100, "RUB");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Zero_HasZeroAmount()
    {
        Money.Zero.Amount.Should().Be(0);
        Money.Zero.Currency.Should().Be("RUB");
        Money.Zero.IsZero.Should().BeTrue();
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var result = Money.Rub(100).Add(Money.Rub(50));
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("RUB");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsDomainException()
    {
        var act = () => Money.Rub(100).Add(Money.Of(50, "USD"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ReturnsDifference()
    {
        var result = Money.Rub(100).Subtract(Money.Rub(40));
        result.Amount.Should().Be(60);
    }

    [Fact]
    public void Subtract_ResultNegative_ThrowsDomainException()
    {
        var act = () => Money.Rub(50).Subtract(Money.Rub(100));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Subtract_DifferentCurrency_ThrowsDomainException()
    {
        var act = () => Money.Rub(100).Subtract(Money.Of(50, "USD"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Multiply_PositiveQuantity_ReturnsProduct()
    {
        var result = Money.Rub(100).Multiply(3);
        result.Amount.Should().Be(300);
    }

    [Fact]
    public void Multiply_ZeroQuantity_ReturnsZero()
    {
        var result = Money.Rub(100).Multiply(0);
        result.Amount.Should().Be(0);
    }

    [Fact]
    public void Multiply_NegativeQuantity_ThrowsDomainException()
    {
        var act = () => Money.Rub(100).Multiply(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsGreaterThan_LargerAmount_ReturnsTrue()
    {
        Money.Rub(100).IsGreaterThan(Money.Rub(50)).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_SmallerAmount_ReturnsFalse()
    {
        Money.Rub(50).IsGreaterThan(Money.Rub(100)).Should().BeFalse();
    }

    [Fact]
    public void IsGreaterThan_DifferentCurrency_ThrowsDomainException()
    {
        var act = () => Money.Rub(100).IsGreaterThan(Money.Of(50, "USD"));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameAmountAndCurrency_ReturnsTrue()
    {
        Money.Rub(100).Should().Be(Money.Rub(100));
    }

    [Fact]
    public void Equals_DifferentAmount_ReturnsFalse()
    {
        Money.Rub(100).Should().NotBe(Money.Rub(200));
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var str = Money.Rub(1000).ToString();
        str.Should().Contain("1").And.Contain("RUB");
    }
}