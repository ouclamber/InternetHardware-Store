using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class CatalogProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IProductAttributeRepository> _attributeRepo = new();

    private static Product CreateProduct(int id = 1, string name = "MacBook", decimal price = 1000, int categoryId = 1)
    {
        var product = new Product(
            ProductName.Create(name),
            Money.Rub(price),
            brandId: 1,
            categoryId: categoryId,
            typeId: 1,
            description: "Test");

        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    private static ProductAttribute CreateAttribute(int id = 1, string name = "Процессор")
    {
        var attr = new ProductAttribute(name, "Основные", "ГГц");

        var idProp = typeof(ProductAttribute).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(attr, id);

        return attr;
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsId()
    {
        var handler = new CreateProductHandler(_productRepo.Object);
        _productRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var command = new CreateProductCommand
        {
            Name = "MacBook Air",
            Price = 99999,
            BrandId = 1,
            CategoryId = 1,
            TypeId = 1,
            Description = "Test"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_InvalidName_ThrowsDomainException()
    {
        var handler = new CreateProductHandler(_productRepo.Object);
        var command = new CreateProductCommand
        {
            Name = "",
            Price = 1000,
            BrandId = 1, CategoryId = 1, TypeId = 1
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateProduct_NegativePrice_ThrowsDomainException()
    {
        var handler = new CreateProductHandler(_productRepo.Object);
        var command = new CreateProductCommand
        {
            Name = "Test",
            Price = -100,
            BrandId = 1, CategoryId = 1, TypeId = 1
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddProductImage_ValidData_AddsImage()
    {
        var handler = new AddProductImageHandler(_productRepo.Object);
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _productRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var command = new AddProductImageCommand(1, "/uploads/img.jpg", "Test", true);
        await handler.Handle(command, CancellationToken.None);

        product.Images.Should().HaveCount(1);
        _productRepo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddProductImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new AddProductImageHandler(_productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new AddProductImageCommand(999, "/test.jpg", null, false);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteProductImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new DeleteProductImageHandler(_productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new DeleteProductImageCommand(999, 1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task SetMainImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new SetMainImageHandler(_productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new SetMainImageCommand(999, 1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddAttributeValue_ValidData_AddsValue()
    {
        var handler = new AddAttributeValueHandler(_productRepo.Object, _attributeRepo.Object);
        var product = CreateProduct();
        var attribute = CreateAttribute();

        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _attributeRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(attribute);
        _productRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

        var command = new AddAttributeValueCommand(1, 1, "Apple M2");
        await handler.Handle(command, CancellationToken.None);

        _productRepo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAttributeValue_ProductNotFound_ThrowsDomainException()
    {
        var handler = new AddAttributeValueHandler(_productRepo.Object, _attributeRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new AddAttributeValueCommand(999, 1, "value");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddAttributeValue_AttributeNotFound_ThrowsDomainException()
    {
        var handler = new AddAttributeValueHandler(_productRepo.Object, _attributeRepo.Object);
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _attributeRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((ProductAttribute?)null);

        var command = new AddAttributeValueCommand(1, 999, "value");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateAttributeValue_ProductNotFound_ThrowsDomainException()
    {
        var handler = new UpdateAttributeValueHandler(_productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new UpdateAttributeValueCommand(999, 1, "new");
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteAttributeValue_ProductNotFound_ThrowsDomainException()
    {
        var handler = new DeleteAttributeValueHandler(_productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var command = new DeleteAttributeValueCommand(999, 1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task SearchProducts_ValidQuery_ReturnsResults()
    {
        var handler = new SearchProductsHandler(_productRepo.Object);
        var products = new List<Product> { CreateProduct(1, "MSI", 1000), CreateProduct(2, "ASUS", 2000) };
        _productRepo.Setup(r => r.SearchAsync("msi", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(products);

        var result = await handler.Handle(new SearchProductsQuery("msi"), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProducts_EmptyQuery_ReturnsEmpty()
    {
        var handler = new SearchProductsHandler(_productRepo.Object);
        var result = await handler.Handle(new SearchProductsQuery(""), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchProducts_FiltersByMinPrice()
    {
        var handler = new SearchProductsHandler(_productRepo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1500) };
        _productRepo.Setup(r => r.SearchAsync("x", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(products);

        var query = new SearchProductsQuery("x") { MinPrice = 1000 };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchProducts_FiltersByCategory()
    {
        var handler = new SearchProductsHandler(_productRepo.Object);
        var products = new List<Product>
        {
            CreateProduct(1, "A", 500, categoryId: 1),
            CreateProduct(2, "B", 1000, categoryId: 2)
        };
        _productRepo.Setup(r => r.SearchAsync("x", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(products);

        var query = new SearchProductsQuery("x") { CategoryId = 2 };
        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProducts_NoLimit_ReturnsAll()
    {
        var handler = new GetProductsHandler(_productRepo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1000) };
        _productRepo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(products);

        var result = await handler.Handle(new GetProductsQuery { Limit = 0 }, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProducts_Limit_AppliesTake()
    {
        var handler = new GetProductsHandler(_productRepo.Object);
        var products = new List<Product>
        {
            CreateProduct(1, "A", 500),
            CreateProduct(2, "B", 1000),
            CreateProduct(3, "C", 1500)
        };
        _productRepo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(products);

        var result = await handler.Handle(new GetProductsQuery { Limit = 2 }, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateProductAttribute_ValidData_ReturnsId()
    {
        var handler = new CreateProductAttributeHandler(_attributeRepo.Object);
        _attributeRepo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
        _attributeRepo.Setup(r => r.AddAsync(It.IsAny<ProductAttribute>(), It.IsAny<CancellationToken>()))
                      .Returns(Task.CompletedTask);

        var command = new CreateProductAttributeCommand("Процессор", "Основные", "ГГц");
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _attributeRepo.Verify(r => r.AddAsync(It.IsAny<ProductAttribute>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAttribute_DuplicateName_ThrowsDomainException()
    {
        var handler = new CreateProductAttributeHandler(_attributeRepo.Object);
        _attributeRepo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        var command = new CreateProductAttributeCommand("Процессор", null, null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}