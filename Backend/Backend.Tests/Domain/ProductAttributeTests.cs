using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ProductAttributeTests
{
    [Fact]
    public void Constructor_ValidData_CreatesAttribute()
    {
        var attr = new ProductAttribute("Процессор", "Основные", "ГГц");

        attr.Name.Should().Be("Процессор");
        attr.AttributeGroup.Should().Be("Основные");
        attr.Unit.Should().Be("ГГц");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyName_ThrowsDomainException(string name)
    {
        var act = () => new ProductAttribute(name);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TooLongName_ThrowsDomainException()
    {
        var act = () => new ProductAttribute(new string('a', 101));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var attr = new ProductAttribute("  Процессор  ");
        attr.Name.Should().Be("Процессор");
    }

    [Fact]
    public void Update_ModifiesFields()
    {
        var attr = new ProductAttribute("Old");

        attr.Update("New", "Group", "Unit");

        attr.Name.Should().Be("New");
        attr.AttributeGroup.Should().Be("Group");
        attr.Unit.Should().Be("Unit");
    }
}