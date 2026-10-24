using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Handlers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using Backend.Tests.TestHelpers;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Catalog.Handlers;

/// <summary>
/// Управление товаром: создание с проверкой ссылок, редактирование, склад, активация, каталог с пагинацией.
/// </summary>
public class ProductManagementHandlerTests
{
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IBrandRepository> _brands = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ITypeRepository> _types = new();

    private void ReferencesExist()
    {
        _brands.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Brand("B"));
        _categories.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Category("C"));
        _types.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ProductType("T"));
    }

    private CreateProductHandler CreateHandler() => new(_products.Object, _brands.Object, _categories.Object, _types.Object);
    private UpdateProductHandler UpdateHandler() => new(_products.Object, _brands.Object, _categories.Object, _types.Object);

    private static CreateProductCommand ValidCreate() => new()
    {
        Name = "MacBook Air",
        Description = "13 дюймов",
        Price = 99999,
        BrandId = 1,
        CategoryId = 2,
        TypeId = 3,
        StockQuantity = 12
    };

    // === Создание ===

    [Fact]
    public async Task Create_WithStock_SavesProductWithStock()
    {
        ReferencesExist();
        Product? saved = null;
        _products.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
                 .Callback<Product, CancellationToken>((p, _) => saved = p)
                 .Returns(Task.CompletedTask);

        await CreateHandler().Handle(ValidCreate(), CancellationToken.None);

        saved!.StockQuantity.Should().Be(12);
        saved.BrandId.Should().Be(1);
        saved.CategoryId.Should().Be(2);
        saved.TypeId.Should().Be(3);
    }

    [Fact]
    public async Task Create_UnknownBrand_ThrowsDomainException_InsteadOfDbError()
    {
        ReferencesExist();
        _brands.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Brand?)null);

        var act = async () => await CreateHandler().Handle(ValidCreate(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Бренд*");
        _products.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_UnknownCategory_Throws()
    {
        ReferencesExist();
        _categories.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var act = async () => await CreateHandler().Handle(ValidCreate(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Категория*");
    }

    [Fact]
    public async Task Create_UnknownType_Throws()
    {
        ReferencesExist();
        _types.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync((ProductType?)null);

        var act = async () => await CreateHandler().Handle(ValidCreate(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Тип*");
    }

    [Fact]
    public async Task Create_NegativeStock_Throws()
    {
        ReferencesExist();
        var command = ValidCreate();
        command.StockQuantity = -5;

        var act = async () => await CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    // === Редактирование ===

    [Fact]
    public async Task Update_ChangesAllFields()
    {
        ReferencesExist();
        var product = TestData.Product(id: 5, name: "Old", price: 100);
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await UpdateHandler().Handle(new UpdateProductCommand
        {
            ProductId = 5,
            Name = "New name",
            Description = "New description",
            Price = 250,
            BrandId = 7,
            CategoryId = 8,
            TypeId = 9
        }, CancellationToken.None);

        product.Name.Value.Should().Be("New name");
        product.Description.Should().Be("New description");
        product.Price.Amount.Should().Be(250);
        product.BrandId.Should().Be(7);
        product.CategoryId.Should().Be(8);
        product.TypeId.Should().Be(9);
        _products.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidPrice_LeavesProductUntouched()
    {
        ReferencesExist();
        var product = TestData.Product(id: 5, name: "Old", price: 100);
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = async () => await UpdateHandler().Handle(new UpdateProductCommand
        {
            ProductId = 5, Name = "New", Price = 0, BrandId = 1, CategoryId = 1, TypeId = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        product.Name.Value.Should().Be("Old");
        _products.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_NotFound_ThrowsNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = async () => await UpdateHandler().Handle(new UpdateProductCommand { ProductId = 404, Name = "X", Price = 1 }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // === Склад и активация ===

    [Fact]
    public async Task SetStock_UpdatesStock()
    {
        var product = TestData.Product(id: 3, stock: 1);
        _products.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await new SetProductStockHandler(_products.Object).Handle(new SetProductStockCommand(3, 40), CancellationToken.None);

        product.StockQuantity.Should().Be(40);
        _products.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetStock_Negative_Throws()
    {
        var product = TestData.Product(id: 3, stock: 1);
        _products.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = async () => await new SetProductStockHandler(_products.Object).Handle(new SetProductStockCommand(3, -1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task SetStock_NotFound_ThrowsNotFound()
    {
        var act = async () => await new SetProductStockHandler(_products.Object).Handle(new SetProductStockCommand(9, 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Activate_InactiveProduct_Activates()
    {
        var product = TestData.Product(id: 3, active: false);
        _products.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await new ActivateProductHandler(_products.Object).Handle(new ActivateProductCommand(3), CancellationToken.None);

        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_AlreadyActive_Throws()
    {
        var product = TestData.Product(id: 3);
        _products.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var act = async () => await new ActivateProductHandler(_products.Object).Handle(new ActivateProductCommand(3), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    // === Каталог с пагинацией ===

    [Fact]
    public async Task GetPaged_BuildsNormalizedQuery_AndMapsResult()
    {
        ProductQuery? captured = null;
        _products.Setup(r => r.GetPagedAsync(It.IsAny<ProductQuery>(), It.IsAny<CancellationToken>()))
                 .Callback<ProductQuery, CancellationToken>((q, _) => captured = q)
                 .ReturnsAsync(new PagedResult<Product>(new[] { TestData.Product(id: 1, stock: 3) }, 25, 2, 12));

        var result = await new GetProductsPagedHandler(_products.Object).Handle(new GetProductsPagedQuery
        {
            CategoryId = 4,
            BrandIds = new[] { 1, 1, 2 },
            MinPrice = 100,
            MaxPrice = 5000,
            Search = "  phone ",
            InStockOnly = true,
            Sort = "PRICE_DESC",
            Page = 2,
            PageSize = 12
        }, CancellationToken.None);

        captured!.CategoryId.Should().Be(4);
        captured.BrandIds.Should().BeEquivalentTo(new[] { 1, 2 });
        captured.Search.Should().Be("phone");
        captured.InStockOnly.Should().BeTrue();
        captured.IncludeInactive.Should().BeFalse();
        captured.Sort.Should().Be(ProductSort.PriceDesc);

        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Items.Should().ContainSingle();
        result.Items[0].InStock.Should().BeTrue();
        result.Items[0].StockQuantity.Should().Be(3);
    }

    [Theory]
    [InlineData(null, ProductSort.Default)]
    [InlineData("relevance", ProductSort.Default)]
    [InlineData("price_asc", ProductSort.PriceAsc)]
    [InlineData("name_desc", ProductSort.NameDesc)]
    [InlineData("newest", ProductSort.Newest)]
    public void ParseSort_KnownValues(string? sort, ProductSort expected)
    {
        GetProductsPagedHandler.ParseSort(sort).Should().Be(expected);
    }

    [Fact]
    public void ParseSort_Unknown_Throws()
    {
        var act = () => GetProductsPagedHandler.ParseSort("random");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public async Task GetPaged_MinGreaterThanMax_ThrowsBeforeQueryingDb()
    {
        var act = async () => await new GetProductsPagedHandler(_products.Object)
            .Handle(new GetProductsPagedQuery { MinPrice = 10, MaxPrice = 1 }, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _products.Verify(r => r.GetPagedAsync(It.IsAny<ProductQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
