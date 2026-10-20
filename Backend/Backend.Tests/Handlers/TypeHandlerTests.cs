using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class TypeHandlerTests
{
    private readonly Mock<ITypeRepository> _repo = new();

    [Fact]
    public async Task CreateType_ValidData_ReturnsId()
    {
        var handler = new CreateTypeHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<ProductType>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var result = await handler.Handle(new CreateTypeCommand("Ноутбук"), CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _repo.Verify(r => r.AddAsync(It.IsAny<ProductType>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateType_DuplicateName_ThrowsDomainException()
    {
        var handler = new CreateTypeHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new CreateTypeCommand("Ноутбук"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateType_ValidData_Updates()
    {
        var handler = new UpdateTypeHandler(_repo.Object);
        var type = new ProductType("Ноутбук");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(type);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<ProductType>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new UpdateTypeCommand(1, "Ноутбуки"), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(type, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateType_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateTypeHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((ProductType?)null);

        var act = async () => await handler.Handle(new UpdateTypeCommand(999, "X"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateType_DuplicateName_ThrowsDomainException()
    {
        var handler = new UpdateTypeHandler(_repo.Object);
        var type = new ProductType("Ноутбук");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(type);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new UpdateTypeCommand(1, "Смартфон"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}