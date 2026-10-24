using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ProductNameTests
{

    [Fact]
    public void Create_ValidName_CreatesProductName()
    {
        var name = ProductName.Create("Apple MacBook Air M2");
        name.Value.Should().Be("Apple MacBook Air M2");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var name = ProductName.Create("  MacBook Air  ");
        name.Value.Should().Be("MacBook Air");
    }

    [Fact]
    public void Create_MaxLengthName_Succeeds()
    {
        var text = new string('a', 200);
        var name = ProductName.Create(text);
        name.Value.Length.Should().Be(200);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Empty_ThrowsDomainException(string? value)
    {
        var act = () => ProductName.Create(value!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLong_ThrowsDomainException()
    {
        var act = () => ProductName.Create(new string('a', 201));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameName_ReturnsTrue()
    {
        ProductName.Create("MacBook").Should().Be(ProductName.Create("MacBook"));
    }

    [Fact]
    public void Equals_DifferentName_ReturnsFalse()
    {
        ProductName.Create("MacBook").Should().NotBe(ProductName.Create("ASUS ROG"));
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        string value = ProductName.Create("MacBook");
        value.Should().Be("MacBook");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        ProductName.Create("MacBook").ToString().Should().Be("MacBook");
    }
}