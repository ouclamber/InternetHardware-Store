using Backend.Domain.Catalog.Entities;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class BrandRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly BrandRepository _repo;

    public BrandRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new BrandRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_ValidBrand_Saves()
    {
        await _repo.AddAsync(new Brand("Apple"));
        _context.Brands.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsBrand()
    {
        var brand = new Brand("Apple");
        await _repo.AddAsync(brand);

        var found = await _repo.GetByIdAsync(brand.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Apple");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Existing_ReturnsBrand()
    {
        await _repo.AddAsync(new Brand("Apple"));

        var found = await _repo.GetByNameAsync("Apple");

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByNameAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByNameAsync("Nonexistent");
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_OrderedByName()
    {
        await _repo.AddAsync(new Brand("Sony"));
        await _repo.AddAsync(new Brand("Apple"));
        await _repo.AddAsync(new Brand("Samsung"));

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(3);
        all[0].Name.Should().Be("Apple");
        all[1].Name.Should().Be("Samsung");
        all[2].Name.Should().Be("Sony");
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        await _repo.AddAsync(new Brand("Apple"));

        var exists = await _repo.ExistsAsync("Apple");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync("Nonexistent");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesBrand()
    {
        var brand = new Brand("Apple");
        await _repo.AddAsync(brand);

        brand.Rename("Apple Inc");
        await _repo.UpdateAsync(brand);

        var found = await _repo.GetByIdAsync(brand.Id);
        found!.Name.Should().Be("Apple Inc");
    }

    [Fact]
    public async Task DeleteAsync_RemovesBrand()
    {
        var brand = new Brand("Apple");
        await _repo.AddAsync(brand);

        await _repo.DeleteAsync(brand);

        _context.Brands.Should().BeEmpty();
    }
}