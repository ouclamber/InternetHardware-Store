using Backend.Domain.Catalog.Entities;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ProductImageTests
{
    [Fact]
    public void Constructor_ValidUrl_CreatesImage()
    {
        var img = new ProductImage("/uploads/test.jpg", "Test alt", true);

        img.ImageUrl.Should().Be("/uploads/test.jpg");
        img.AltText.Should().Be("Test alt");
        img.IsMain.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyUrl_ThrowsDomainException(string url)
    {
        var act = () => new ProductImage(url);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TooLongUrl_ThrowsDomainException()
    {
        var act = () => new ProductImage(new string('a', 501));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TrimsUrl()
    {
        var img = new ProductImage("  /uploads/test.jpg  ");
        img.ImageUrl.Should().Be("/uploads/test.jpg");
    }

    [Fact]
    public void Update_ModifiesUrlAndAlt()
    {
        var img = new ProductImage("/img.jpg");
        img.Update("/new.jpg", "New alt");

        img.ImageUrl.Should().Be("/new.jpg");
        img.AltText.Should().Be("New alt");
    }

    [Fact]
    public void MakeMain_SetsIsMainTrue()
    {
        var img = new ProductImage("/img.jpg");
        img.MakeMain();
        img.IsMain.Should().BeTrue();
    }

    [Fact]
    public void RemoveMainStatus_SetsIsMainFalse()
    {
        var img = new ProductImage("/img.jpg", null, true);
        img.RemoveMainStatus();
        img.IsMain.Should().BeFalse();
    }
}