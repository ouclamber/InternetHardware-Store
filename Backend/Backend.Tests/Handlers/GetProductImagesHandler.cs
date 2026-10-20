using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;   // ← для ProductName
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class GetProductImagesHandlerTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly GetProductImagesHandler _handler;

    public GetProductImagesHandlerTests()
    {
        _handler = new GetProductImagesHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WithExistingProduct_ReturnsImagesOrderedByMain()
    {
        var product = new Product(
            ProductName.Create("Test"),
            Money.Rub(1000),
            brandId: 1, categoryId: 1, typeId: 1);

        product.AddImage("/a.jpg", "A", isMain: false);
        product.AddImage("/main.jpg", "Main", isMain: true);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var result = await _handler.Handle(new GetProductImagesQuery(1), default);

        result.Should().HaveCount(2);
        result[0].IsMain.Should().BeTrue();
        result[0].ImageUrl.Should().Be("/main.jpg");
    }

    [Fact]
    public async Task Handle_WithMissingProduct_ThrowsDomainException()
    {
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Product?)null);

        Func<Task> act = () => _handler.Handle(new GetProductImagesQuery(999), default);

        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*999*");
    }

    [Fact]
    public async Task Handle_WithNoImages_ReturnsEmpty()
    {
        var product = new Product(
            ProductName.Create("Test"),
            Money.Rub(1000),
            brandId: 1, categoryId: 1, typeId: 1);

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(product);

        var result = await _handler.Handle(new GetProductImagesQuery(1), default);

        result.Should().BeEmpty();
    }
}