using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class OrderRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly OrderRepository _repo;
    private readonly Address _address = Address.Create("ул. Ленина 1", "Москва", "101000");

    public OrderRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new OrderRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private Order CreateOrder(int userId = 1, string productName = "Test Product")
    {
        var items = new[]
        {
            (ProductId: 1, ProductName: productName, Quantity: 2, UnitPrice: Money.Rub(1000))
        };
        return new Order(userId, _address, items);
    }

    [Fact]
    public async Task AddAsync_ValidOrder_Saves()
    {
        var order = CreateOrder();
        await _repo.AddAsync(order);

        _context.Orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsOrder()
    {
        var order = CreateOrder();
        await _repo.AddAsync(order);

        var found = await _repo.GetByIdAsync(order.Id);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByNumberAsync_Existing_ReturnsOrder()
    {
        var order = CreateOrder();
        await _repo.AddAsync(order);

        var found = await _repo.GetByNumberAsync(order.OrderNumber.Value);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsUserOrders()
    {
        await _repo.AddAsync(CreateOrder(userId: 1));
        await _repo.AddAsync(CreateOrder(userId: 1));
        await _repo.AddAsync(CreateOrder(userId: 2));

        var orders = await _repo.GetByUserIdAsync(1);

        orders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_NoOrders_ReturnsEmpty()
    {
        var orders = await _repo.GetByUserIdAsync(999);
        orders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        await _repo.AddAsync(CreateOrder(userId: 1));
        await _repo.AddAsync(CreateOrder(userId: 2));
        await _repo.AddAsync(CreateOrder(userId: 3));

        var orders = await _repo.GetAllAsync();

        orders.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsMatchingStatus()
    {
        var pending1 = CreateOrder(userId: 1);
        var pending2 = CreateOrder(userId: 2);
        var paid = CreateOrder(userId: 3);
        paid.MarkAsPaid();

        await _repo.AddAsync(pending1);
        await _repo.AddAsync(pending2);
        await _repo.AddAsync(paid);

        var pending = await _repo.GetByStatusAsync(OrderStatus.Pending);
        var paidOrders = await _repo.GetByStatusAsync(OrderStatus.Paid);

        pending.Should().HaveCount(2);
        paidOrders.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsLatestN()
    {
        for (int i = 1; i <= 5; i++)
            await _repo.AddAsync(CreateOrder(userId: i));

        var recent = await _repo.GetRecentAsync(3);

        recent.Should().HaveCount(3);
    }

    [Fact]
    public async Task CountAsync_ReturnsTotalCount()
    {
        await _repo.AddAsync(CreateOrder(userId: 1));
        await _repo.AddAsync(CreateOrder(userId: 2));

        var count = await _repo.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesOrder()
    {
        var order = CreateOrder();
        await _repo.AddAsync(order);

        order.MarkAsPaid();
        await _repo.UpdateAsync(order);

        var found = await _repo.GetByIdAsync(order.Id);
        found!.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task AddAndClearCartAsync_SavesOrderAndClearsCart()
    {
        // Подготовка: товар и корзина с позицией
        var brand = new Backend.Domain.Catalog.Entities.Brand("TestBrand");
        var category = new Backend.Domain.Catalog.Entities.Category("TestCategory");
        var type = new Backend.Domain.Catalog.Entities.ProductType("TestType");
        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.Types.Add(type);
        _context.SaveChanges();

        var product = new Backend.Domain.Catalog.Entities.Product(
            Backend.Domain.Catalog.ValueObjects.ProductName.Create("Test Product"),
            Money.Rub(1000),
            brand.Id, category.Id, type.Id,
            stockQuantity: 10);
        _context.Products.Add(product);
        _context.SaveChanges();

        var cart = new Backend.Domain.Sales.CartAggregate.Cart(userId: 1);
        cart.AddItem(product, 2);
        _context.Carts.Add(cart);
        _context.SaveChanges();

        var order = CreateOrder(userId: 1);

        await _repo.AddAndClearCartAsync(order);

        _context.Orders.Should().HaveCount(1);
        _context.CartItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAndClearCartAsync_NoCart_StillSavesOrder()
    {
        var order = CreateOrder(userId: 5);

        await _repo.AddAndClearCartAsync(order);

        _context.Orders.Should().HaveCount(1);
    }
}
