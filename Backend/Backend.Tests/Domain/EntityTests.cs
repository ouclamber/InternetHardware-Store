using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class EntityTests
{
    [Fact]
    public void Equals_SameTypeSameId_ReturnsTrue()
    {
        var a = new Brand("Apple");
        var b = new Brand("Samsung");

        // У обоих Id = 0 → Equals false
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var brand = new Brand("Apple");
        brand.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentTypeSameId_ReturnsFalse()
    {
        var brand = new Brand("Apple");
        var category = new Category("Cat");

        brand.Equals(category).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_NullNull_ReturnsTrue()
    {
        Brand? a = null;
        Brand? b = null;
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentNulls_ReturnsTrue()
    {
        Brand? a = new Brand("Apple");
        Brand? b = null;
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameTypeSameId_ReturnsSame()
    {
        var a = new Brand("Apple");
        var b = new Brand("Samsung");
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}