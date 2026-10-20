using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class BrandHandlerTests
{
    private readonly Mock<IBrandRepository> _repo = new();

    [Fact]
    public async Task CreateBrand_ValidData_ReturnsId()
    {
        var handler = new CreateBrandHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        var result = await handler.Handle(new CreateBrandCommand("Apple"), CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _repo.Verify(r => r.AddAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBrand_DuplicateName_ThrowsDomainException()
    {
        var handler = new CreateBrandHandler(_repo.Object);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new CreateBrandCommand("Apple"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateBrand_ValidData_Updates()
    {
        var handler = new UpdateBrandHandler(_repo.Object);
        var brand = new Brand("Apple");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(brand);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new UpdateBrandCommand(1, "Apple Inc"), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(brand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBrand_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateBrandHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Brand?)null);

        var act = async () => await handler.Handle(new UpdateBrandCommand(999, "X"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateBrand_DuplicateName_ThrowsDomainException()
    {
        var handler = new UpdateBrandHandler(_repo.Object);
        var brand = new Brand("Apple");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(brand);
        _repo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new UpdateBrandCommand(1, "Samsung"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteBrand_ValidData_Deletes()
    {
        var handler = new DeleteBrandHandler(_repo.Object);
        var brand = new Brand("Apple");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(brand);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.DeleteAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);

        await handler.Handle(new DeleteBrandCommand(1), CancellationToken.None);

        _repo.Verify(r => r.DeleteAsync(brand, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBrand_HasProducts_ThrowsDomainException()
    {
        var handler = new DeleteBrandHandler(_repo.Object);
        var brand = new Brand("Apple");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(brand);
        _repo.Setup(r => r.HasProductsAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var act = async () => await handler.Handle(new DeleteBrandCommand(1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteBrand_NotFound_ThrowsDomainException()
    {
        var handler = new DeleteBrandHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Brand?)null);

        var act = async () => await handler.Handle(new DeleteBrandCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetBrandById_Existing_ReturnsDto()
    {
        var handler = new GetBrandByIdHandler(_repo.Object);
        var brand = new Brand("Apple");
        _repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
             .ReturnsAsync(brand);

        var result = await handler.Handle(new GetBrandByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBrandById_NotFound_ReturnsNull()
    {
        var handler = new GetBrandByIdHandler(_repo.Object);
        _repo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Brand?)null);

        var result = await handler.Handle(new GetBrandByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}