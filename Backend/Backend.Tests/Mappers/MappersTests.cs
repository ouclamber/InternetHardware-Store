using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Handlers;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using System.Reflection;
using Backend.Application.DTOs;
using Backend.Application.Catalog.Mappers;

namespace Backend.Tests.Mappers;

public class MappersTests
{
    private static T SetId<T>(T entity, int id) where T : class
    {
        var prop = typeof(T).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        prop?.SetValue(entity, id);
        return entity;
    }

    [Fact]
    public void BrandMapper_MapsAllFields()
    {
        var brand = SetId(new Brand("Apple"), 5);

        var dto = BrandMapper.MapToDto(brand);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(5);
        dto.Name.Should().Be("Apple");
    }

    [Fact]
    public void CategoryMapper_MapsAllFields()
    {
        var category = SetId(new Category("Ноутбуки", "Все ноутбуки", "/img.jpg"), 3);

        var dto = CategoryMapper.MapToDto(category);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(3);
        dto.Name.Should().Be("Ноутбуки");
    }

    [Fact]
    public void ProductTypeMapper_MapsAllFields()
    {
        var type = SetId(new ProductType("Ноутбук"), 7);

        var dto = ProductTypeMapper.MapToDto(type);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(7);
        dto.Name.Should().Be("Ноутбук");
    }

    [Fact]
    public void ProductMapper_MapsBasicFields()
    {
        var product = new Product(
            ProductName.Create("MacBook Air"),
            Money.Rub(99999),
            brandId: 1,
            categoryId: 2,
            typeId: 3,
            description: "Test description");
        SetId(product, 10);

        var dto = ProductMapper.MapToDto(product);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(10);
        dto.Name.Should().Be("MacBook Air");
        dto.Price.Should().Be(99999);
        dto.BrandId.Should().Be(1);
        dto.CategoryId.Should().Be(2);
        dto.TypeId.Should().Be(3);
    }

    [Fact]
    public void ProductMapper_HandlesNullImages()
    {
        var product = new Product(
            ProductName.Create("Test"),
            Money.Rub(1000),
            brandId: 1,
            categoryId: 1,
            typeId: 1);
        SetId(product, 1);

        var dto = ProductMapper.MapToDto(product);

        dto.Should().NotBeNull();
        dto.Images.Should().BeEmpty();
    }

    [Fact]
    public void ProductMapper_HandlesNullDescription()
    {
        var product = new Product(
            ProductName.Create("Test"),
            Money.Rub(1000),
            brandId: 1,
            categoryId: 1,
            typeId: 1,
            description: null);
        SetId(product, 1);

        var dto = ProductMapper.MapToDto(product);

        dto.Should().NotBeNull();
    }

    [Fact]
    public void ProductImageMapper_MapsFields()
    {
        var image = new ProductImage("/uploads/img.jpg", "Test alt", true);
        SetId(image, 42);

        var dto = ProductImageMapper.MapToDto(image);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(42);
        dto.ImageUrl.Should().Be("/uploads/img.jpg");
        dto.AltText.Should().Be("Test alt");
        dto.IsMain.Should().BeTrue();
    }

    [Fact]
    public void ProductAttributeMapper_MapsFields()
    {
        var attr = new ProductAttribute("Процессор", "Основные", "ГГц");
        SetId(attr, 8);

        var dto = ProductAttributeMapper.MapToDto(attr);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(8);
        dto.Name.Should().Be("Процессор");
    }
}