using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class CatalogMoreHandlerTests
{
    private readonly Mock<IBrandRepository> _brandRepo = new();
    private readonly Mock<ITypeRepository> _typeRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();

    private static Brand CreateBrand(int id = 1, string name = "Apple")
    {
        var brand = new Brand(name);
        var idProp = typeof(Brand).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(brand, id);
        return brand;
    }

    private static ProductType CreateType(int id = 1, string name = "Ноутбук")
    {
        var type = new ProductType(name);
        var idProp = typeof(ProductType).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(type, id);
        return type;
    }

    [Fact]
    public async Task GetBrands_ReturnsAll()
    {
        var handler = new GetBrandsHandler(_brandRepo.Object);
        _brandRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Brand> { CreateBrand(), CreateBrand(2, "Samsung") });

        var result = await handler.Handle(new GetBrandsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBrands_Empty_ReturnsEmptyList()
    {
        var handler = new GetBrandsHandler(_brandRepo.Object);
        _brandRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Brand>());

        var result = await handler.Handle(new GetBrandsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTypes_ReturnsAll()
    {
        var handler = new GetTypesHandler(_typeRepo.Object);
        _typeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ProductType> { CreateType(), CreateType(2, "Смартфон") });

        var result = await handler.Handle(new GetTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTypes_Empty_ReturnsEmptyList()
    {
        var handler = new GetTypesHandler(_typeRepo.Object);
        _typeRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<ProductType>());

        var result = await handler.Handle(new GetTypesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCategories_All_ReturnsAll()
    {
        var handler = new GetCategoriesHandler(_categoryRepo.Object);
        _categoryRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Category> { new Category("Cat1"), new Category("Cat2") });

        var result = await handler.Handle(new GetCategoriesQuery { RootOnly = false }, CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategories_RootOnly_ReturnsRoot()
    {
        var handler = new GetCategoriesHandler(_categoryRepo.Object);
        _categoryRepo.Setup(r => r.GetRootAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<Category> { new Category("Root") });

        var result = await handler.Handle(new GetCategoriesQuery { RootOnly = true }, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetBrandById_Existing_ReturnsDto()
    {
        var handler = new GetBrandByIdHandler(_brandRepo.Object);
        _brandRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(CreateBrand());

        var result = await handler.Handle(new GetBrandByIdQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBrandById_NotFound_ReturnsNull()
    {
        var handler = new GetBrandByIdHandler(_brandRepo.Object);
        _brandRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Brand?)null);

        var result = await handler.Handle(new GetBrandByIdQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateBrand_DuplicateName_ThrowsDomainException()
    {
        var handler = new CreateBrandHandler(_brandRepo.Object);
        _brandRepo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        var act = async () => await handler.Handle(new CreateBrandCommand("Apple"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateBrand_SameName_DoesNotThrow()
    {
        var handler = new UpdateBrandHandler(_brandRepo.Object);
        var brand = CreateBrand(1, "Apple");
        _brandRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(brand);
        _brandRepo.Setup(r => r.UpdateAsync(It.IsAny<Brand>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var act = async () => await handler.Handle(new UpdateBrandCommand(1, "Apple"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}