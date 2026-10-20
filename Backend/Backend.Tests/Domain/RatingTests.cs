using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class RatingTests
{

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Of_ValidValue_CreatesRating(int value)
    {
        var rating = Rating.Of(value);
        rating.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Of_OutOfRange_ThrowsDomainException(int value)
    {
        var act = () => Rating.Of(value);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void One_Returns1() => Rating.One.Value.Should().Be(1);

    [Fact]
    public void Two_Returns2() => Rating.Two.Value.Should().Be(2);

    [Fact]
    public void Three_Returns3() => Rating.Three.Value.Should().Be(3);

    [Fact]
    public void Four_Returns4() => Rating.Four.Value.Should().Be(4);

    [Fact]
    public void Five_Returns5() => Rating.Five.Value.Should().Be(5);

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(3, false)]
    [InlineData(1, false)]
    public void IsPositive_ReturnsExpected(int value, bool expected)
    {
        Rating.Of(value).IsPositive.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(5, false)]
    public void IsNegative_ReturnsExpected(int value, bool expected)
    {
        Rating.Of(value).IsNegative.Should().Be(expected);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        Rating.Of(5).Should().Be(Rating.Of(5));
    }

    [Fact]
    public void ImplicitConversion_ToInt_ReturnsValue()
    {
        int value = Rating.Of(4);
        value.Should().Be(4);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        Rating.Of(5).ToString().Should().Be("5");
    }
}