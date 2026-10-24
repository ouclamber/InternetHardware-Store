using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ProductAttributeValueTests
{
    [Fact]
    public void Constructor_ValidData_Creates()
    {
        var val = new ProductAttributeValue(1, 2, "Apple M2");

        val.ProductId.Should().Be(1);
        val.AttributeId.Should().Be(2);
        val.Value.Should().Be("Apple M2");
    }

    [Fact]
    public void Constructor_NullValue_ThrowsDomainException()
    {
        var act = () => new ProductAttributeValue(1, 2, null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TooLongValue_ThrowsDomainException()
    {
        var act = () => new ProductAttributeValue(1, 2, new string('a', 501));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateValue_ChangesValue()
    {
        var val = new ProductAttributeValue(1, 2, "old");
        val.UpdateValue("New value");
        val.Value.Should().Be("New value");
    }
}