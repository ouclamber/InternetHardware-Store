using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class ProductImageHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();

    private static Product CreateProduct(int id = 1)
    {
        var product = new Product(
            ProductName.Create("Test"),
            Money.Rub(1000),
            brandId: 1,
            categoryId: 1,
            typeId: 1);

        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    [Fact]
    public async Task AddProductImage_ValidData_Adds()
    {
        var handler = new AddProductImageHandler(_repo.Object);
        var product = CreateProduct();
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        // (productId, imageUrl, altText, isMain)
        var command = new AddProductImageCommand(1, "/uploads/test.jpg", "Test", true);
        await handler.Handle(command, CancellationToken.None);

        product.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddProductImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new AddProductImageHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        var command = new AddProductImageCommand(999, "/test.jpg", null, false);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task SetMainImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new SetMainImageHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        // (productId, imageId)
        var command = new SetMainImageCommand(999, 1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteProductImage_ProductNotFound_ThrowsDomainException()
    {
        var handler = new DeleteProductImageHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        // (productId, imageId) — аналогично
        var command = new DeleteProductImageCommand(999, 1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}