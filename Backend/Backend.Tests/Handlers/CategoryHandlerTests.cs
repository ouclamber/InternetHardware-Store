using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class CategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _repo = new();

    [Fact]
    public async Task CreateCategory_ValidData_ReturnsId()
    {
        var handler = new CreateCategoryHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var command = new CreateCategoryCommand("Новая категория", null, null, null);
        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _repo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_ThrowsDomainException()
    {
        var handler = new CreateCategoryHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var command = new CreateCategoryCommand("Существующая", null, null, null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateCategory_InvalidParentId_ThrowsDomainException()
    {
        var handler = new CreateCategoryHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Category?)null);

        var command = new CreateCategoryCommand("Дочерняя", null, null, 999);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateCategory_ValidData_Updates()
    {
        var handler = new UpdateCategoryHandler(_repo.Object);
        var category = new Category("Старое имя");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var command = new UpdateCategoryCommand(1, "Новое имя", null, null);
        await handler.Handle(command, CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateCategoryHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Category?)null);

        var command = new UpdateCategoryCommand(999, "Имя", null, null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateCategory_DuplicateName_ThrowsDomainException()
    {
        var handler = new UpdateCategoryHandler(_repo.Object);
        var category = new Category("Старое имя");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var command = new UpdateCategoryCommand(1, "Новое имя", null, null);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    // ============================================================
    // === DELETE CATEGORY ===
    // ============================================================

    [Fact]
    public async Task DeleteCategory_ValidData_Deletes()
    {
        var handler = new DeleteCategoryHandler(_repo.Object);
        var category = new Category("Категория");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.HasSubCategoriesAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        _repo.Verify(r => r.DeleteAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_NotFound_ThrowsDomainException()
    {
        var handler = new DeleteCategoryHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Category?)null);

        var act = async () => await handler.Handle(new DeleteCategoryCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteCategory_HasProducts_ThrowsDomainException()
    {
        var handler = new DeleteCategoryHandler(_repo.Object);
        var category = new Category("Категория");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteCategory_HasSubCategories_ThrowsDomainException()
    {
        var handler = new DeleteCategoryHandler(_repo.Object);
        var category = new Category("Категория");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.HasSubCategoriesAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new DeleteCategoryCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetCategoryById_Existing_ReturnsDto()
    {
        var handler = new GetCategoryByIdHandler(_repo.Object);
        var category = new Category("Категория");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(category);

        var result = await handler.Handle(new GetCategoryByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryById_NotFound_ReturnsNull()
    {
        var handler = new GetCategoryByIdHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Category?)null);

        var result = await handler.Handle(new GetCategoryByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}