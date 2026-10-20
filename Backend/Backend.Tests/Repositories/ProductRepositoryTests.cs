using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repo;
    private int _brandId;
    private int _categoryId;
    private int _typeId;

    public ProductRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new ProductRepository(_context);

        // Создаём базовые сущности и сохраняем их Id
        var brand = new Brand("TestBrand");
        var category = new Category("TestCategory");
        var type = new ProductType("TestType");
        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.Types.Add(type);
        _context.SaveChanges();

        _brandId = brand.Id;
        _categoryId = category.Id;
        _typeId = type.Id;
    }

    public void Dispose() => _context.Dispose();

    private Product CreateProduct(string name = "Test Product", decimal price = 1000, bool isActive = true)
    {
        var product = new Product(
            ProductName.Create(name),
            Money.Rub(price),
            _brandId,
            _categoryId,
            _typeId,
            "Test description");

        if (!isActive)
            product.Deactivate();

        return product;
    }

    [Fact]
    public async Task AddAsync_ValidProduct_Saves()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        _context.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsProduct()
    {
        var product = CreateProduct("MacBook");
        await _repo.AddAsync(product);

        var found = await _repo.GetByIdAsync(product.Id);

        found.Should().NotBeNull();
        found!.Name.Value.Should().Be("MacBook");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActive()
    {
        await _repo.AddAsync(CreateProduct("Active1", isActive: true));
        await _repo.AddAsync(CreateProduct("Active2", isActive: true));
        await _repo.AddAsync(CreateProduct("Inactive", isActive: false));

        var active = await _repo.GetActiveAsync();

        active.Should().HaveCount(2);
        active.Should().OnlyContain(p => p.IsActive);
    }

    [Fact]
    public async Task GetActiveAsync_EmptyDb_ReturnsEmpty()
    {
        var active = await _repo.GetActiveAsync();
        active.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsActiveInCategory()
    {
        await _repo.AddAsync(CreateProduct("Prod1"));
        await _repo.AddAsync(CreateProduct("Prod2"));

        var found = await _repo.GetByCategoryAsync(_categoryId);

        found.Should().HaveCount(2);
        found.Should().OnlyContain(p => p.CategoryId == _categoryId);
    }

    [Fact]
    public async Task GetByCategoryAsync_WrongCategory_ReturnsEmpty()
    {
        await _repo.AddAsync(CreateProduct("Prod1"));

        var found = await _repo.GetByCategoryAsync(999);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByBrandAsync_ReturnsActiveInBrand()
    {
        await _repo.AddAsync(CreateProduct("Prod1"));
        await _repo.AddAsync(CreateProduct("Prod2"));

        var found = await _repo.GetByBrandAsync(_brandId);

        found.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByBrandAsync_WrongBrand_ReturnsEmpty()
    {
        await _repo.AddAsync(CreateProduct("Prod1"));

        var found = await _repo.GetByBrandAsync(999);

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        var exists = await _repo.ExistsAsync(product.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync(999);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ReturnsTotalCount()
    {
        await _repo.AddAsync(CreateProduct("Prod1"));
        await _repo.AddAsync(CreateProduct("Prod2"));
        await _repo.AddAsync(CreateProduct("Prod3"));

        var count = await _repo.CountAsync();

        count.Should().Be(3);
    }

    [Fact]
    public async Task CountActiveAsync_CountsOnlyActive()
    {
        await _repo.AddAsync(CreateProduct("Active1"));
        await _repo.AddAsync(CreateProduct("Active2"));
        await _repo.AddAsync(CreateProduct("Inactive", isActive: false));

        var count = await _repo.CountActiveAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesProduct()
    {
        var product = CreateProduct("Old name");
        await _repo.AddAsync(product);

        product.Rename(ProductName.Create("New name"));
        await _repo.UpdateAsync(product);

        var found = await _repo.GetByIdAsync(product.Id);
        found!.Name.Value.Should().Be("New name");
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var product = CreateProduct();
        await _repo.AddAsync(product);

        await _repo.DeleteAsync(product);

        _context.Products.Should().BeEmpty();
    }
}