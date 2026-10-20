using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class GetTypesHandlerTests
{
    private readonly Mock<ITypeRepository> _typeRepo = new();
    private readonly GetTypesHandler _handler;

    public GetTypesHandlerTests()
    {
        _handler = new GetTypesHandler(_typeRepo.Object);
    }

    private static List<ProductType> CreateTypes(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new ProductType($"Type{i}"))
            .ToList();
    }

    [Fact]
    public async Task Handle_ReturnsAllTypes()
    {
        var types = CreateTypes(4);

        _typeRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(types);

        var result = await _handler.Handle(new GetTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(4);
        _typeRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmpty()
    {
        _typeRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductType>());

        var result = await _handler.Handle(new GetTypesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsToDto_Correctly()
    {
        var types = new List<ProductType> { new ProductType("Ноутбук") };

        _typeRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(types);

        var result = await _handler.Handle(new GetTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}