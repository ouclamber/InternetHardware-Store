using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using System.Reflection;

namespace Backend.Tests.Domain;

public class ProductTests
{
    private static Product CreateValidProduct()
    {
        return new Product(
            ProductName.Create("MacBook Air"),
            Money.Rub(99999),
            brandId: 1,
            categoryId: 2,
            typeId: 3,
            description: "Test");
    }

    private static void SetId(Entity entity, int id)
    {
        var prop = typeof(Entity).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        prop?.SetValue(entity, id);
    }

    [Fact]
    public void Constructor_WithValidData_CreatesActiveProduct()
    {
        var product = CreateValidProduct();

        product.IsActive.Should().BeTrue();
        product.Name.Value.Should().Be("MacBook Air");
        product.Price.Amount.Should().Be(99999);
        product.BrandId.Should().Be(1);
        product.CategoryId.Should().Be(2);
        product.TypeId.Should().Be(3);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithNullName_Throws()
    {
        Action act = () => new Product(null!, Money.Rub(100), 1, 1, 1);
        act.Should().Throw<DomainException>().WithMessage("*Название*");
    }

    [Fact]
    public void Constructor_WithNullPrice_Throws()
    {
        Action act = () => new Product(ProductName.Create("X"), null!, 1, 1, 1);
        act.Should().Throw<DomainException>().WithMessage("*Цена*");
    }

    [Fact]
    public void Constructor_WithZeroPrice_Throws()
    {
        Action act = () => new Product(ProductName.Create("X"), Money.Rub(0), 1, 1, 1);
        act.Should().Throw<DomainException>().WithMessage("*больше 0*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidBrandId_Throws(int brandId)
    {
        Action act = () => new Product(ProductName.Create("X"), Money.Rub(1), brandId, 1, 1);
        act.Should().Throw<DomainException>().WithMessage("*бренда*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidCategoryId_Throws(int categoryId)
    {
        Action act = () => new Product(ProductName.Create("X"), Money.Rub(1), 1, categoryId, 1);
        act.Should().Throw<DomainException>().WithMessage("*категории*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidTypeId_Throws(int typeId)
    {
        Action act = () => new Product(ProductName.Create("X"), Money.Rub(1), 1, 1, typeId);
        act.Should().Throw<DomainException>().WithMessage("*типа*");
    }

    [Fact]
    public void Rename_UpdatesNameAndTimestamp()
    {
        var product = CreateValidProduct();

        product.Rename(ProductName.Create("New Name"));

        product.Name.Value.Should().Be("New Name");
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdatePrice_UpdatesPrice()
    {
        var product = CreateValidProduct();

        product.UpdatePrice(Money.Rub(50000));

        product.Price.Amount.Should().Be(50000);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDescription_WithNull_SetsEmpty()
    {
        var product = CreateValidProduct();

        product.UpdateDescription(null);

        product.Description.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDescription_TrimsWhitespace()
    {
        var product = CreateValidProduct();

        product.UpdateDescription("  hello  ");

        product.Description.Should().Be("hello");
    }

    [Fact]
    public void ChangeBrand_UpdatesBrandId()
    {
        var product = CreateValidProduct();
        product.ChangeBrand(99);
        product.BrandId.Should().Be(99);
    }

    [Fact]
    public void ChangeCategory_UpdatesCategoryId()
    {
        var product = CreateValidProduct();
        product.ChangeCategory(99);
        product.CategoryId.Should().Be(99);
    }

    [Fact]
    public void ChangeType_UpdatesTypeId()
    {
        var product = CreateValidProduct();
        product.ChangeType(99);
        product.TypeId.Should().Be(99);
    }

    [Fact]
    public void Deactivate_SetsInactive()
    {
        var product = CreateValidProduct();

        product.Deactivate();

        product.IsActive.Should().BeFalse();
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Throws()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        Action act = () => product.Deactivate();

        act.Should().Throw<DomainException>().WithMessage("*уже деактивирован*");
    }

    [Fact]
    public void Activate_ReactivatesProduct()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        product.Activate();

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_Throws()
    {
        var product = CreateValidProduct();

        Action act = () => product.Activate();

        act.Should().Throw<DomainException>().WithMessage("*уже активен*");
    }

    [Fact]
    public void AddImage_AddsToList()
    {
        var product = CreateValidProduct();

        product.AddImage("/img.jpg", "alt", false);

        product.Images.Should().HaveCount(1);
    }

    [Fact]
    public void AddImage_WithIsMain_RemovesMainFromOthers()
    {
        var product = CreateValidProduct();
        product.AddImage("/a.jpg", "A", isMain: true);
        product.AddImage("/b.jpg", "B", isMain: true); // должен снять main с первого

        var mainImages = product.Images.Where(i => i.IsMain).ToList();

        mainImages.Should().HaveCount(1);
        mainImages[0].ImageUrl.Should().Be("/b.jpg");
    }

    [Fact]
    public void AddImage_WhenMoreThan10_Throws()
    {
        var product = CreateValidProduct();
        for (int i = 0; i < 10; i++)
            product.AddImage($"/{i}.jpg");

        Action act = () => product.AddImage("/11.jpg");

        act.Should().Throw<DomainException>().WithMessage("*10 изображений*");
    }

    [Fact]
    public void RemoveImage_RemovesById()
    {
        var product = CreateValidProduct();
        SetId(product, 1);
        product.AddImage("/a.jpg");
        var imageId = product.Images.First().Id;

        product.RemoveImage(imageId);

        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void RemoveImage_WithInvalidId_Throws()
    {
        var product = CreateValidProduct();

        Action act = () => product.RemoveImage(999);

        act.Should().Throw<DomainException>().WithMessage("*не найдено*");
    }

    [Fact]
    public void MakeImageMain_SetsMainAndRemovesOthers()
    {
        var product = CreateValidProduct();
        SetId(product, 1);
        product.AddImage("/a.jpg", isMain: true);
        product.AddImage("/b.jpg", isMain: false);
        var secondId = product.Images.Last().Id;

        product.MakeImageMain(secondId);

        product.Images.First(i => i.Id == secondId).IsMain.Should().BeTrue();
        product.Images.Count(i => i.IsMain).Should().Be(1);
    }

    [Fact]
    public void MakeImageMain_WithInvalidId_Throws()
    {
        var product = CreateValidProduct();

        Action act = () => product.MakeImageMain(999);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddAttributeValue_AddsToList()
    {
        var product = CreateValidProduct();
        SetId(product, 1);

        product.AddAttributeValue(attributeId: 5, value: "16GB");

        product.Values.Should().HaveCount(1);
        product.Values.First().AttributeId.Should().Be(5);
    }

    [Fact]
    public void AddAttributeValue_WithDuplicateAttributeId_Throws()
    {
        var product = CreateValidProduct();
        SetId(product, 1);
        product.AddAttributeValue(5, "16GB");

        Action act = () => product.AddAttributeValue(5, "32GB");

        act.Should().Throw<DomainException>().WithMessage("*уже добавлен*");
    }

    [Fact]
    public void UpdateAttributeValue_UpdatesExisting()
    {
        var product = CreateValidProduct();
        SetId(product, 1);
        product.AddAttributeValue(5, "16GB");

        product.UpdateAttributeValue(5, "32GB");

        product.Values.First().Value.Should().Be("32GB");
    }

    [Fact]
    public void UpdateAttributeValue_WithMissing_Throws()
    {
        var product = CreateValidProduct();

        Action act = () => product.UpdateAttributeValue(999, "x");

        act.Should().Throw<DomainException>().WithMessage("*не найден*");
    }

    [Fact]
    public void RemoveAttributeValue_RemovesExisting()
    {
        var product = CreateValidProduct();
        SetId(product, 1);
        product.AddAttributeValue(5, "16GB");

        product.RemoveAttributeValue(5);

        product.Values.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAttributeValue_WithMissing_Throws()
    {
        var product = CreateValidProduct();

        Action act = () => product.RemoveAttributeValue(999);

        act.Should().Throw<DomainException>().WithMessage("*не найден*");
    }
}