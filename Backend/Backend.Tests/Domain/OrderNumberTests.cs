using Backend.Domain.Shared;
using Backend.Domain.Sales.ValueObjects;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace Backend.Tests.Domain;

public class OrderNumberTests
{

    [Fact]
    public void FromString_ValidFormat_CreatesOrderNumber()
    {
        var on = OrderNumber.FromString("ORD-20261009-ABCD1234");
        on.Value.Should().Be("ORD-20261009-ABCD1234");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromString_Empty_ThrowsDomainException(string? value)
    {
        var act = () => OrderNumber.FromString(value!);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("ORD-2026-ABCD1234")]           // мало цифр в дате
    [InlineData("ORD-20261009-abcd1234")]       // lowercase
    [InlineData("ORD-20261009-ABCD123")]        // мало в hex
    [InlineData("XXX-20261009-ABCD1234")]       // неверный префикс
    [InlineData("ORD-2026100X-ABCD1234")]       // не-цифра в дате
    public void FromString_InvalidFormat_ThrowsDomainException(string value)
    {
        var act = () => OrderNumber.FromString(value);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FromString_TrimsWhitespace()
    {
        var on = OrderNumber.FromString("  ORD-20261009-ABCD1234  ");
        on.Value.Should().Be("ORD-20261009-ABCD1234");
    }

    [Fact]
    public void Generate_ProducesValidOrderNumber()
    {
        var on = OrderNumber.Generate();
        var regex = new Regex(@"^ORD-\d{8}-[A-F0-9]{8}$");
        regex.IsMatch(on.Value).Should().BeTrue();
    }

    [Fact]
    public void Generate_IsUnique()
    {
        var a = OrderNumber.Generate();
        var b = OrderNumber.Generate();
        a.Value.Should().NotBe(b.Value);
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = OrderNumber.FromString("ORD-20261009-ABCD1234");
        var b = OrderNumber.FromString("ORD-20261009-ABCD1234");
        a.Should().Be(b);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        string value = OrderNumber.FromString("ORD-20261009-ABCD1234");
        value.Should().Be("ORD-20261009-ABCD1234");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        OrderNumber.FromString("ORD-20261009-ABCD1234")
            .ToString().Should().Be("ORD-20261009-ABCD1234");
    }
}