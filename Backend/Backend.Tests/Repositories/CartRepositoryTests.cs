using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Shared;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class CartRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CartRepository _repo;
    private readonly Product _testProduct;

    public CartRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new CartRepository(_context);

        // Создаём тестовый товар
        var brand = new Brand("TestBrand");
        var category = new Category("TestCategory");
        var type = new ProductType("TestType");
        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.Types.Add(type);
        _context.SaveChanges();

        _testProduct = new Product(
            ProductName.Create("Test Product"),
            Money.Rub(1000),
            brand.Id,
            category.Id,
            type.Id);
        _context.Products.Add(_testProduct);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    private Cart CreateCart(int userId = 1) => new Cart(userId);

    [Fact]
    public async Task AddAsync_ValidCart_Saves()
    {
        var cart = CreateCart();
        await _repo.AddAsync(cart);

        _context.Carts.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_Existing_ReturnsCart()
    {
        var cart = CreateCart(userId: 1);
        await _repo.AddAsync(cart);

        var found = await _repo.GetByUserIdAsync(1);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetByUserIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByUserIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsCart()
    {
        var cart = CreateCart();
        await _repo.AddAsync(cart);

        var found = await _repo.GetByIdAsync(cart.Id);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesCart()
    {
        var cart = CreateCart();
        await _repo.AddAsync(cart);

        cart.AddItem(_testProduct, 3);
        await _repo.UpdateAsync(cart);

        var found = await _repo.GetByUserIdAsync(cart.UserId);
        found!.Items.Should().HaveCount(1);
        found.Items.First().Quantity.Value.Should().Be(3);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCart()
    {
        var cart = CreateCart();
        await _repo.AddAsync(cart);

        await _repo.DeleteAsync(cart);

        _context.Carts.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearByUserIdAsync_Existing_ClearsItems()
    {
        var cart = CreateCart(userId: 1);
        cart.AddItem(_testProduct, 5);
        await _repo.AddAsync(cart);

        await _repo.ClearByUserIdAsync(1);

        var found = await _repo.GetByUserIdAsync(1);
        found!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearByUserIdAsync_NonExisting_DoesNothing()
    {
        await _repo.ClearByUserIdAsync(999);

        _context.Carts.Should().BeEmpty();
    }
}