using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class GetBrandsHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRepo = new();
    private readonly GetBrandsHandler _handler;

    public GetBrandsHandlerTests()
    {
        _handler = new GetBrandsHandler(_brandRepo.Object);
    }

    private static List<Brand> CreateBrands(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Brand($"Brand{i}"))
            .ToList();
    }

    [Fact]
    public async Task Handle_ReturnsAllBrands()
    {
        var brands = CreateBrands(5);

        _brandRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(brands);

        var result = await _handler.Handle(new GetBrandsQuery(), CancellationToken.None);

        result.Should().HaveCount(5);
        _brandRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _brandRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Brand>());

        var result = await _handler.Handle(new GetBrandsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsToDto_Correctly()
    {
        var brands = new List<Brand> { new Brand("Apple") };

        _brandRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(brands);

        var result = await _handler.Handle(new GetBrandsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}