using Backend.Domain.Shared;
using Backend.Domain.Sales.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class QuantityTests
{

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(999)]
    public void Of_ValidValue_CreatesQuantity(int value)
    {
        var q = Quantity.Of(value);
        q.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Of_NonPositive_ThrowsDomainException(int value)
    {
        var act = () => Quantity.Of(value);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Of_TooLarge_ThrowsDomainException()
    {
        var act = () => Quantity.Of(1000);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void One_ReturnsQuantityWithValue1()
    {
        Quantity.One.Value.Should().Be(1);
    }

    [Fact]
    public void Add_PositiveDelta_ReturnsSum()
    {
        var q = Quantity.Of(5).Add(3);
        q.Value.Should().Be(8);
    }

    [Fact]
    public void Add_NegativeDelta_ThrowsDomainException()
    {
        var act = () => Quantity.Of(5).Add(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Add_ExceedsMax_ThrowsDomainException()
    {
        var act = () => Quantity.Of(999).Add(1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Subtract_PositiveDelta_ReturnsDifference()
    {
        var q = Quantity.Of(10).Subtract(3);
        q.Value.Should().Be(7);
    }

    [Fact]
    public void Subtract_NegativeDelta_ThrowsDomainException()
    {
        var act = () => Quantity.Of(5).Subtract(-1);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Subtract_ResultZero_ThrowsDomainException()
    {
        var act = () => Quantity.Of(5).Subtract(5);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ImplicitConversion_ToInt_ReturnsValue()
    {
        int value = Quantity.Of(5);
        value.Should().Be(5);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        Quantity.Of(5).Should().Be(Quantity.Of(5));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        Quantity.Of(5).ToString().Should().Be("5");
    }
}