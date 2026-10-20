using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class GetCategoriesHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly GetCategoriesHandler _handler;

    public GetCategoriesHandlerTests()
    {
        _handler = new GetCategoriesHandler(_categoryRepo.Object);
    }

    private static List<Category> CreateCategories(int count = 3)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Category($"Категория {i}", $"Описание {i}"))
            .ToList();
    }

    [Fact]
    public async Task Handle_RootOnlyFalse_ReturnsAllCategories()
    {
        var categories = CreateCategories(5);

        _categoryRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.Handle(
            new GetCategoriesQuery { RootOnly = false },
            CancellationToken.None);

        result.Should().HaveCount(5);
        _categoryRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepo.Verify(r => r.GetRootAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RootOnlyTrue_ReturnsRootCategories()
    {
        var categories = CreateCategories(2);

        _categoryRepo
            .Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.Handle(
            new GetCategoriesQuery { RootOnly = true },
            CancellationToken.None);

        result.Should().HaveCount(2);
        _categoryRepo.Verify(r => r.GetRootAsync(It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyCategories_ReturnsEmptyList()
    {
        _categoryRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        var result = await _handler.Handle(
            new GetCategoriesQuery { RootOnly = false },
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsToDto_Correctly()
    {
        var categories = new List<Category>
        {
            new Category("Ноутбуки", "Все ноутбуки")
        };

        _categoryRepo
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.Handle(
            new GetCategoriesQuery { RootOnly = false },
            CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Should().NotBeNull();
    }
}