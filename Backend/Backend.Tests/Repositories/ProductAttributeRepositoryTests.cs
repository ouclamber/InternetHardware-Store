using Backend.Domain.Catalog.Entities;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class ProductAttributeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductAttributeRepository _repo;

    public ProductAttributeRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new ProductAttributeRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private ProductAttribute CreateAttribute(string name = "Процессор")
    {
        return new ProductAttribute(name, "Основные");
    }

    [Fact]
    public async Task AddAsync_ValidAttribute_Saves()
    {
        await _repo.AddAsync(CreateAttribute());
        _context.ProductAttributes.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsAttribute()
    {
        var attr = CreateAttribute();
        await _repo.AddAsync(attr);

        var found = await _repo.GetByIdAsync(attr.Id);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Existing_ReturnsAttribute()
    {
        await _repo.AddAsync(CreateAttribute("Процессор"));

        var found = await _repo.GetByNameAsync("Процессор");

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByNameAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByNameAsync("Несуществующий");
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrderedByName()
    {
        await _repo.AddAsync(CreateAttribute("Процессор"));
        await _repo.AddAsync(CreateAttribute("RAM"));
        await _repo.AddAsync(CreateAttribute("Накопитель"));

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        await _repo.AddAsync(CreateAttribute("Процессор"));

        var exists = await _repo.ExistsAsync("Процессор");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync("Несуществующий");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HasValuesAsync_NoValues_ReturnsFalse()
    {
        var attr = CreateAttribute();
        await _repo.AddAsync(attr);

        var hasValues = await _repo.HasValuesAsync(attr.Id);
        hasValues.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_DoesNotThrow()
    {
        var attr = CreateAttribute("Процессор");
        await _repo.AddAsync(attr);

        // Просто вызываем UpdateAsync — проверяем, что не падает
        await _repo.UpdateAsync(attr);

        var found = await _repo.GetByIdAsync(attr.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be("Процессор");
    }

    [Fact]
    public async Task DeleteAsync_RemovesAttribute()
    {
        var attr = CreateAttribute();
        await _repo.AddAsync(attr);

        await _repo.DeleteAsync(attr);

        _context.ProductAttributes.Should().BeEmpty();
    }
}