using Backend.Domain.Catalog.Entities;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CategoryRepository _repo;

    public CategoryRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new CategoryRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_ValidCategory_Saves()
    {
        var category = new Category("Ноутбуки", "Все ноутбуки");
        await _repo.AddAsync(category);

        _context.Categories.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsCategory()
    {
        var category = new Category("Ноутбуки");
        await _repo.AddAsync(category);

        var found = await _repo.GetByIdAsync(category.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Ноутбуки");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Existing_ReturnsCategory()
    {
        await _repo.AddAsync(new Category("Ноутбуки"));

        var found = await _repo.GetByNameAsync("Ноутбуки");

        found.Should().NotBeNull();
        found!.Name.Should().Be("Ноутбуки");
    }

    [Fact]
    public async Task GetByNameAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByNameAsync("Несуществующая");
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrderedByName()
    {
        await _repo.AddAsync(new Category("B"));
        await _repo.AddAsync(new Category("A"));
        await _repo.AddAsync(new Category("C"));

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(3);
        all[0].Name.Should().Be("A");
        all[1].Name.Should().Be("B");
        all[2].Name.Should().Be("C");
    }

    [Fact]
    public async Task GetRootAsync_ReturnsOnlyRootCategories()
    {
        var root = new Category("Root");
        await _repo.AddAsync(root);

        var child = new Category("Child");
        child.SetParent(root);
        await _repo.AddAsync(child);

        var roots = await _repo.GetRootAsync();

        roots.Should().HaveCount(1);
        roots[0].Name.Should().Be("Root");
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        await _repo.AddAsync(new Category("Ноутбуки"));

        var exists = await _repo.ExistsAsync("Ноутбуки");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync("Несуществующая");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HasSubCategoriesAsync_WithChildren_ReturnsTrue()
    {
        var parent = new Category("Parent");
        await _repo.AddAsync(parent);

        var child = new Category("Child");
        child.SetParent(parent);
        await _repo.AddAsync(child);

        var hasChildren = await _repo.HasSubCategoriesAsync(parent.Id);
        hasChildren.Should().BeTrue();
    }

    [Fact]
    public async Task HasSubCategoriesAsync_NoChildren_ReturnsFalse()
    {
        var category = new Category("Категория");
        await _repo.AddAsync(category);

        var hasChildren = await _repo.HasSubCategoriesAsync(category.Id);
        hasChildren.Should().BeFalse();
    }

    [Fact]
    public async Task HasProductsAsync_NoProducts_ReturnsFalse()
    {
        var category = new Category("Категория");
        await _repo.AddAsync(category);

        var hasProducts = await _repo.HasProductsAsync(category.Id);
        hasProducts.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesCategory()
    {
        var category = new Category("Старое имя");
        await _repo.AddAsync(category);

        category.Rename("Новое имя");
        await _repo.UpdateAsync(category);

        var found = await _repo.GetByIdAsync(category.Id);
        found!.Name.Should().Be("Новое имя");
    }

    [Fact]
    public async Task DeleteAsync_RemovesCategory()
    {
        var category = new Category("Ноутбуки");
        await _repo.AddAsync(category);

        await _repo.DeleteAsync(category);

        _context.Categories.Should().BeEmpty();
    }
}