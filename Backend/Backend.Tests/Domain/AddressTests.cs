using Backend.Domain.Shared;
using Backend.Domain.Sales.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class AddressTests
{

    [Fact]
    public void Create_ValidData_CreatesAddress()
    {
        var addr = Address.Create("ул. Ленина 1", "Москва", "101000");
        addr.Street.Should().Be("ул. Ленина 1");
        addr.City.Should().Be("Москва");
        addr.PostalCode.Should().Be("101000");
    }

    [Fact]
    public void Create_WithoutPostalCode_Succeeds()
    {
        var addr = Address.Create("ул. Ленина 1", "Москва");
        addr.PostalCode.Should().BeNull();
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var addr = Address.Create("  ул. Ленина 1  ", "  Москва  ", "  101000  ");
        addr.Street.Should().Be("ул. Ленина 1");
        addr.City.Should().Be("Москва");
        addr.PostalCode.Should().Be("101000");
    }

    [Theory]
    [InlineData("", "Москва")]
    [InlineData("   ", "Москва")]
    [InlineData(null, "Москва")]
    public void Create_EmptyStreet_ThrowsDomainException(string? street, string city)
    {
        var act = () => Address.Create(street!, city);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ул. Ленина 1", "")]
    [InlineData("ул. Ленина 1", "   ")]
    [InlineData("ул. Ленина 1", null)]
    public void Create_EmptyCity_ThrowsDomainException(string street, string? city)
    {
        var act = () => Address.Create(street, city!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_StreetTooLong_ThrowsDomainException()
    {
        var act = () => Address.Create(new string('a', 201), "Москва");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_CityTooLong_ThrowsDomainException()
    {
        var act = () => Address.Create("ул. Ленина 1", new string('a', 101));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_PostalCodeTooLong_ThrowsDomainException()
    {
        var act = () => Address.Create("ул. Ленина 1", "Москва", new string('1', 21));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ToDisplayString_FullAddress_ReturnsFull()
    {
        var addr = Address.Create("ул. Ленина 1", "Москва", "101000");
        addr.ToDisplayString().Should().Be("Москва, ул. Ленина 1, 101000");
    }

    [Fact]
    public void ToDisplayString_WithoutPostal_ReturnsWithoutPostal()
    {
        var addr = Address.Create("ул. Ленина 1", "Москва");
        addr.ToDisplayString().Should().Be("Москва, ул. Ленина 1");
    }

    [Fact]
    public void ToString_EqualsToDisplayString()
    {
        var addr = Address.Create("ул. Ленина 1", "Москва", "101000");
        addr.ToString().Should().Be(addr.ToDisplayString());
    }

    [Fact]
    public void Equals_SameData_ReturnsTrue()
    {
        var a = Address.Create("ул. Ленина 1", "Москва", "101000");
        var b = Address.Create("ул. Ленина 1", "Москва", "101000");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentStreet_ReturnsFalse()
    {
        var a = Address.Create("ул. Ленина 1", "Москва");
        var b = Address.Create("ул. Ленина 2", "Москва");
        a.Should().NotBe(b);
    }
}