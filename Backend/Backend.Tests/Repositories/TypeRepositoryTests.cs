using Backend.Domain.Catalog.Entities;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class TypeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TypeRepository _repo;

    public TypeRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new TypeRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task AddAsync_ValidType_Saves()
    {
        await _repo.AddAsync(new ProductType("Ноутбук"));
        _context.Types.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsType()
    {
        var type = new ProductType("Ноутбук");
        await _repo.AddAsync(type);

        var found = await _repo.GetByIdAsync(type.Id);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Ноутбук");
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByNameAsync_Existing_ReturnsType()
    {
        await _repo.AddAsync(new ProductType("Ноутбук"));

        var found = await _repo.GetByNameAsync("Ноутбук");

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTypes()
    {
        await _repo.AddAsync(new ProductType("Ноутбук"));
        await _repo.AddAsync(new ProductType("Смартфон"));

        var all = await _repo.GetAllAsync();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        await _repo.AddAsync(new ProductType("Ноутбук"));

        var exists = await _repo.ExistsAsync("Ноутбук");
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesType()
    {
        var type = new ProductType("Ноутбук");
        await _repo.AddAsync(type);

        type.Rename("Ноутбуки");
        await _repo.UpdateAsync(type);

        var found = await _repo.GetByIdAsync(type.Id);
        found!.Name.Should().Be("Ноутбуки");
    }

    [Fact]
    public async Task DeleteAsync_RemovesType()
    {
        var type = new ProductType("Ноутбук");
        await _repo.AddAsync(type);

        await _repo.DeleteAsync(type);

        _context.Types.Should().BeEmpty();
    }
}