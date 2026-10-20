using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class ProductHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();

    private static Product CreateProduct(string name = "MacBook", decimal price = 1000)
    {
        return new Product(
            ProductName.Create(name),
            Money.Rub(price),
            brandId: 1,
            categoryId: 1,
            typeId: 1,
            description: "Test");
    }

    [Fact]
    public async Task CreateProduct_ValidData_ReturnsId()
    {
        var handler = new CreateProductHandler(_repo.Object);
        _repo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
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
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_InvalidName_ThrowsDomainException()
    {
        var handler = new CreateProductHandler(_repo.Object);
        var command = new CreateProductCommand
        {
            Name = "",
            Price = 1000,
            BrandId = 1,
            CategoryId = 1,
            TypeId = 1
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateProduct_NegativePrice_ThrowsDomainException()
    {
        var handler = new CreateProductHandler(_repo.Object);
        var command = new CreateProductCommand
        {
            Name = "Test",
            Price = -100,
            BrandId = 1,
            CategoryId = 1,
            TypeId = 1
        };

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateProductPrice_ValidData_Updates()
    {
        var handler = new UpdateProductPriceHandler(_repo.Object);
        var product = CreateProduct();
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new UpdateProductPriceCommand(1, 2000), CancellationToken.None);

        product.Price.Amount.Should().Be(2000);
        _repo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductPrice_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateProductPriceHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = async () => await handler.Handle(new UpdateProductPriceCommand(999, 2000), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateProductPrice_NegativePrice_ThrowsDomainException()
    {
        var handler = new UpdateProductPriceHandler(_repo.Object);
        var product = CreateProduct();
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var act = async () => await handler.Handle(new UpdateProductPriceCommand(1, -100), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeactivateProduct_ValidData_Deactivates()
    {
        var handler = new DeactivateProductHandler(_repo.Object);
        var product = CreateProduct();
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new DeactivateProductCommand(1), CancellationToken.None);

        product.IsActive.Should().BeFalse();
        _repo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateProduct_NotFound_ThrowsDomainException()
    {
        var handler = new DeactivateProductHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var act = async () => await handler.Handle(new DeactivateProductCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetProductById_Existing_ReturnsDto()
    {
        var handler = new GetProductByIdHandler(_repo.Object);
        var product = CreateProduct();
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var result = await handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductById_NotFound_ReturnsNull()
    {
        var handler = new GetProductByIdHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var result = await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsByCategory_ValidId_ReturnsProducts()
    {
        var handler = new GetProductsByCategoryHandler(_repo.Object);
        var products = new List<Product> { CreateProduct("P1"), CreateProduct("P2") };
        _repo.Setup(r => r.GetByCategoryAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(products);

        var result = await handler.Handle(new GetProductsByCategoryQuery(1), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProductsByCategory_InvalidId_ThrowsArgumentException()
    {
        var handler = new GetProductsByCategoryHandler(_repo.Object);
        var act = async () => await handler.Handle(new GetProductsByCategoryQuery(0), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}