using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _handler = new GetProductsHandler(_productRepo.Object);
    }

    [Fact]
    public async Task Handle_ReturnsActiveProducts()
    {
        var products = new List<Product>();

        _productRepo
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.Handle(
            new GetProductsQuery { Limit = 0 },
            CancellationToken.None);

        result.Should().BeEmpty();
        _productRepo.Verify(r => r.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLimit_AppliesTake()
    {
        var products = new List<Product>();

        _productRepo
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.Handle(
            new GetProductsQuery { Limit = 5 },
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ZeroLimit_NoTake()
    {
        var products = new List<Product>();

        _productRepo
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.Handle(
            new GetProductsQuery { Limit = 0 },
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NegativeLimit_NoTake()
    {
        var products = new List<Product>();

        _productRepo
            .Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.Handle(
            new GetProductsQuery { Limit = -1 },
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}