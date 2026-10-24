using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Catalog.Handlers;

public class DeleteTypeHandlerTests
{
    private readonly Mock<ITypeRepository> _repo = new();
    private readonly DeleteTypeHandler _handler;

    public DeleteTypeHandlerTests()
    {
        _handler = new DeleteTypeHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WithValidType_DeletesIt()
    {
        // Arrange
        var type = new ProductType("Ноутбук");

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(type);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.DeleteAsync(type, It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(new DeleteTypeCommand(1), default);

        // Assert
        _repo.Verify(r => r.DeleteAsync(type, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTypeMissing_Throws()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((ProductType?)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteTypeCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*999*");

        _repo.Verify(r => r.DeleteAsync(It.IsAny<ProductType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTypeHasProducts_Throws()
    {
        // Arrange
        var type = new ProductType("Ноутбук");

        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(type);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteTypeCommand(1), default);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
                 .WithMessage("*привязаны товары*");

        _repo.Verify(r => r.DeleteAsync(It.IsAny<ProductType>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}