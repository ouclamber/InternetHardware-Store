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

public class ProductSearchHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();

    private static Product CreateProduct(int id, string name, decimal price, int categoryId = 1)
    {
        var product = new Product(
            ProductName.Create(name),
            Money.Rub(price),
            brandId: 1,
            categoryId: categoryId,
            typeId: 1);

        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    [Fact]
    public async Task SearchProducts_ValidQuery_ReturnsResults()
    {
        var handler = new SearchProductsHandler(_repo.Object);
        var products = new List<Product> { CreateProduct(1, "MSI", 1000), CreateProduct(2, "ASUS", 2000) };
        _repo.Setup(r => r.SearchAsync("msi", It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new SearchProductsQuery("msi"), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchProducts_EmptyQuery_ReturnsEmpty()
    {
        var handler = new SearchProductsHandler(_repo.Object);
        var result = await handler.Handle(new SearchProductsQuery(""), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchProducts_FiltersByMinPrice()
    {
        var handler = new SearchProductsHandler(_repo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1500) };
        _repo.Setup(r => r.SearchAsync("x", It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new SearchProductsQuery("x") { MinPrice = 1000 }, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchProducts_FiltersByMaxPrice()
    {
        var handler = new SearchProductsHandler(_repo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1500) };
        _repo.Setup(r => r.SearchAsync("x", It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new SearchProductsQuery("x") { MaxPrice = 1000 }, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchProducts_FiltersByCategory()
    {
        var handler = new SearchProductsHandler(_repo.Object);
        var products = new List<Product>
        {
            CreateProduct(1, "A", 500, categoryId: 1),
            CreateProduct(2, "B", 1000, categoryId: 2)
        };
        _repo.Setup(r => r.SearchAsync("x", It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new SearchProductsQuery("x") { CategoryId = 2 }, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProducts_NoLimit_ReturnsAll()
    {
        var handler = new GetProductsHandler(_repo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1000) };
        _repo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new GetProductsQuery { Limit = 0 }, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProducts_Limit_AppliesTake()
    {
        var handler = new GetProductsHandler(_repo.Object);
        var products = new List<Product> { CreateProduct(1, "A", 500), CreateProduct(2, "B", 1000), CreateProduct(3, "C", 1500) };
        _repo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new GetProductsQuery { Limit = 2 }, CancellationToken.None);

        result.Should().HaveCount(2);
    }
}