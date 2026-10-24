using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using Backend.Domain.Wishlist;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Repositories;

/// <summary>
/// Регрессионные тесты: каждый «HTTP-запрос» работает со своим DbContext,
/// как в приложении (Scoped), и запросы по умолчанию не отслеживаются.
/// </summary>
public class TrackingRegressionTests
{
    private readonly string _db = Guid.NewGuid().ToString();

    private ApplicationDbContext NewContext() => TestDbContextFactory.CreateLikeProduction(_db);

    private int SeedProduct(int stock = 10, string name = "Product")
    {
        using var ctx = NewContext();
        var brand = new Brand("Brand " + Guid.NewGuid().ToString("N")[..6]);
        var category = new Category("Category " + Guid.NewGuid().ToString("N")[..6]);
        var type = new ProductType("Type " + Guid.NewGuid().ToString("N")[..6]);
        ctx.Brands.Add(brand);
        ctx.Categories.Add(category);
        ctx.Types.Add(type);
        ctx.SaveChanges();

        var product = new Product(ProductName.Create(name), Money.Rub(1000), brand.Id, category.Id, type.Id, stockQuantity: stock);
        product.AddImage("/img/1.webp", isMain: true);
        product.AddImage("/img/2.webp");
        ctx.Products.Add(product);
        ctx.SaveChanges();
        return product.Id;
    }

    private void SeedCart(int userId, params int[] productIds)
    {
        using var ctx = NewContext();
        var cart = new Cart(userId);
        foreach (var id in productIds)
            cart.AddItem(ctx.Products.AsTracking().Single(p => p.Id == id), 1);
        ctx.Carts.Add(cart);
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Cart_RemoveItem_IsPersisted()
    {
        // Раньше Update() на отсоединённом графе не удалял позицию из БД
        var p1 = SeedProduct();
        var p2 = SeedProduct();
        SeedCart(1, p1, p2);

        using (var ctx = NewContext())
        {
            var repo = new CartRepository(ctx);
            var cart = await repo.GetByUserIdAsync(1);
            cart!.RemoveItem(p1);
            await repo.UpdateAsync(cart);
        }

        using var check = NewContext();
        check.CartItems.Count().Should().Be(1);
        check.CartItems.Single().ProductId.Should().Be(p2);
    }

    [Fact]
    public async Task Cart_UpdateQuantityToZero_RemovesItem()
    {
        var p1 = SeedProduct();
        SeedCart(1, p1);

        using (var ctx = NewContext())
        {
            var repo = new CartRepository(ctx);
            var cart = await repo.GetByUserIdAsync(1);
            cart!.UpdateItemQuantity(p1, 0);
            await repo.UpdateAsync(cart);
        }

        using var check = NewContext();
        check.CartItems.Count().Should().Be(0);
    }

    [Fact]
    public async Task Cart_AddItem_DoesNotOverwriteProduct()
    {
        var p1 = SeedProduct();

        using (var ctx = NewContext())
        {
            var products = new ProductRepository(ctx);
            var carts = new CartRepository(ctx);

            var product = await products.GetByIdAsync(p1);
            var cart = new Cart(5);
            await carts.AddAsync(cart);
            cart.AddItem(product!, 2);
            await carts.UpdateAsync(cart);
        }

        using var check = NewContext();
        check.CartItems.Single().Quantity.Value.Should().Be(2);
        check.Products.Single(p => p.Id == p1).Name.Value.Should().Be("Product");
    }

    [Fact]
    public async Task Product_RemoveImage_IsPersisted()
    {
        var productId = SeedProduct();

        using (var ctx = NewContext())
        {
            var repo = new ProductRepository(ctx);
            var product = await repo.GetByIdAsync(productId);
            var mainImage = product!.Images.Single(i => i.IsMain);
            product.RemoveImage(mainImage.Id);
            await repo.UpdateAsync(product);
        }

        using var check = NewContext();
        var images = check.ProductImages.Where(i => i.ProductId == productId).ToList();
        images.Should().HaveCount(1);
        images[0].IsMain.Should().BeTrue();   // главным стало оставшееся изображение
    }

    [Fact]
    public async Task Order_CreateAndCancel_ReservesAndReleasesStockAtomically()
    {
        var productId = SeedProduct(stock: 5);
        SeedCart(1, productId);

        int orderId;
        using (var ctx = NewContext())
        {
            var carts = new CartRepository(ctx);
            var orders = new OrderRepository(ctx);
            var cart = await carts.GetByUserIdAsync(1);
            var item = cart!.Items.Single();

            var order = new Order(1, Address.Create("ул. Ленина 1", "Москва"),
                new[] { (item.ProductId, item.Product!.Name.Value, item.Quantity.Value, item.Product.Price) });
            item.Product.ReserveStock(item.Quantity.Value);
            order.MarkStockReserved();

            await orders.AddAndClearCartAsync(order);
            orderId = order.Id;
        }

        using (var check = NewContext())
        {
            check.Products.Single(p => p.Id == productId).StockQuantity.Should().Be(4);
            check.CartItems.Count().Should().Be(0);
            check.Orders.Single().StockReserved.Should().BeTrue();
        }

        using (var ctx = NewContext())
        {
            var orders = new OrderRepository(ctx);
            var products = new ProductRepository(ctx);
            var order = await orders.GetByIdAsync(orderId);
            order!.Cancel();
            foreach (var (pid, qty) in order.TakeStockToRelease())
                (await products.GetByIdsAsync(new[] { pid })).Single().ReleaseStock(qty);
            await ((Backend.Domain.Shared.IUnitOfWork)ctx).SaveChangesAsync();
        }

        using var final = NewContext();
        final.Products.Single(p => p.Id == productId).StockQuantity.Should().Be(5);
        final.Orders.Single().Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task ConcurrentStockReservation_SecondSave_ThrowsConflict()
    {
        var productId = SeedProduct(stock: 1);

        using var ctx1 = NewContext();
        using var ctx2 = NewContext();
        var p1 = await new ProductRepository(ctx1).GetByIdAsync(productId);
        var p2 = await new ProductRepository(ctx2).GetByIdAsync(productId);

        p1!.ReserveStock(1);
        await ctx1.SaveChangesAsync();

        p2!.ReserveStock(1);   // в памяти второго запроса остаток ещё 1
        var act = async () => await ctx2.SaveChangesAsync();

        await act.Should().ThrowAsync<ConflictException>();

        using var check = NewContext();
        check.Products.Single(p => p.Id == productId).StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Order_Contact_RoundTrips()
    {
        int orderId;
        using (var ctx = NewContext())
        {
            var order = new Order(1, Address.Create("ул. Ленина 1", "Москва"),
                new[] { (1, "Товар", 1, Money.Rub(100)) },
                contact: CustomerContact.Create("Анна", "Смирнова", "anna@example.com", "+79990001122"));
            await new OrderRepository(ctx).AddAsync(order);
            orderId = order.Id;
        }

        using var check = NewContext();
        var loaded = await new OrderRepository(check).GetByIdAsync(orderId);
        loaded!.Contact!.FullName.Should().Be("Анна Смирнова");
        loaded.Contact.Email.Should().Be("anna@example.com");
        loaded.Contact.Phone.Should().Be("+79990001122");
    }
}

public class ProductPagingRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repo;
    private readonly int _phones;
    private readonly int _laptops;
    private readonly int _apple;
    private readonly int _samsung;

    public ProductPagingRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new ProductRepository(_context);

        var apple = new Brand("Apple");
        var samsung = new Brand("Samsung");
        var phones = new Category("Phones");
        var laptops = new Category("Laptops");
        var type = new ProductType("Device");
        _context.AddRange(apple, samsung, phones, laptops, type);
        _context.SaveChanges();

        (_apple, _samsung, _phones, _laptops) = (apple.Id, samsung.Id, phones.Id, laptops.Id);

        Add("iPhone 15", 90000, apple.Id, phones.Id, type.Id, stock: 5);
        Add("Galaxy S24", 70000, samsung.Id, phones.Id, type.Id, stock: 0);
        Add("Galaxy A55", 30000, samsung.Id, phones.Id, type.Id, stock: 3);
        Add("MacBook Air", 120000, apple.Id, laptops.Id, type.Id, stock: 2);
        var hidden = Add("Old phone", 1000, samsung.Id, phones.Id, type.Id, stock: 9);
        hidden.Deactivate();
        _context.SaveChanges();
    }

    private Product Add(string name, decimal price, int brand, int category, int type, int stock)
    {
        var p = new Product(ProductName.Create(name), Money.Rub(price), brand, category, type, stockQuantity: stock);
        _context.Products.Add(p);
        _context.SaveChanges();
        return p;
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetPaged_ByCategory_ExcludesInactive()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { CategoryId = _phones });

        page.TotalCount.Should().Be(3);
        page.Items.Should().OnlyContain(p => p.IsActive && p.CategoryId == _phones);
    }

    [Fact]
    public async Task GetPaged_InStockOnly()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { CategoryId = _phones, InStockOnly = true });

        page.Items.Select(p => p.Name.Value).Should().BeEquivalentTo(new[] { "iPhone 15", "Galaxy A55" });
    }

    [Fact]
    public async Task GetPaged_ByBrands()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { BrandIds = new[] { _apple } });

        page.Items.Select(p => p.Name.Value).Should().BeEquivalentTo(new[] { "iPhone 15", "MacBook Air" });
    }

    [Fact]
    public async Task GetPaged_SortByPriceDesc()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { Sort = ProductSort.PriceDesc });

        page.Items.Select(p => p.Price.Amount).Should().Equal(120000m, 90000m, 70000m, 30000m);
    }

    [Fact]
    public async Task GetPaged_SortByNameAsc()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { CategoryId = _phones, Sort = ProductSort.NameAsc });

        page.Items.Select(p => p.Name.Value).Should().Equal("Galaxy A55", "Galaxy S24", "iPhone 15");
    }

    [Fact]
    public async Task GetPaged_SecondPage()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { Sort = ProductSort.PriceAsc, Page = 2, PageSize = 3 });

        page.TotalCount.Should().Be(4);
        page.TotalPages.Should().Be(2);
        page.Items.Should().ContainSingle();
        page.Items[0].Name.Value.Should().Be("MacBook Air");
    }

    [Fact]
    public async Task GetPaged_NoMatches_ReturnsEmpty()
    {
        var page = await _repo.GetPagedAsync(new ProductQuery { CategoryId = _laptops, BrandIds = new[] { _samsung } });

        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetActiveAsync_Take_LimitsResult()
    {
        var items = await _repo.GetActiveAsync(2);

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountOutOfStockAsync_CountsActiveWithZeroStock()
    {
        (await _repo.CountOutOfStockAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsRequested()
    {
        var ids = _context.Products.Select(p => p.Id).Take(2).ToArray();

        var items = await _repo.GetByIdsAsync(ids);

        items.Select(p => p.Id).Should().BeEquivalentTo(ids);
    }

    [Theory]
    [InlineData("100%", "%100\\%%")]
    [InlineData("a_b", "%a\\_b%")]
    [InlineData("c:\\path", "%c:\\\\path%")]
    public void ToLikePattern_EscapesWildcards(string input, string expected)
    {
        ProductRepository.ToLikePattern(input).Should().Be(expected);
    }
}

public class WishlistAndUserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly WishlistRepository _wishlist;
    private readonly UserRepository _users;
    private readonly User _user;
    private readonly Product _product1;
    private readonly Product _product2;

    public WishlistAndUserRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _wishlist = new WishlistRepository(_context);
        _users = new UserRepository(_context);

        _user = new User(UserName.Create("wisher"), PasswordHash.FromHash(new string('a', 60)));
        var brand = new Brand("B");
        var category = new Category("C");
        var type = new ProductType("T");
        _context.AddRange(_user, brand, category, type);
        _context.SaveChanges();

        _product1 = new Product(ProductName.Create("First"), Money.Rub(10), brand.Id, category.Id, type.Id, stockQuantity: 1);
        _product2 = new Product(ProductName.Create("Second"), Money.Rub(20), brand.Id, category.Id, type.Id, stockQuantity: 1);
        _context.Products.AddRange(_product1, _product2);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Add_Exists_Count_Ids()
    {
        await _wishlist.AddAsync(new WishlistItem(_user.Id, _product1));
        await _wishlist.AddAsync(new WishlistItem(_user.Id, _product2));

        (await _wishlist.ExistsAsync(_user.Id, _product1.Id)).Should().BeTrue();
        (await _wishlist.CountByUserAsync(_user.Id)).Should().Be(2);
        (await _wishlist.GetProductIdsAsync(_user.Id)).Should().BeEquivalentTo(new[] { _product1.Id, _product2.Id });
    }

    [Fact]
    public async Task GetByUser_IncludesProducts()
    {
        await _wishlist.AddAsync(new WishlistItem(_user.Id, _product1));

        var items = await _wishlist.GetByUserAsync(_user.Id);

        items.Should().ContainSingle();
        items[0].Product.Should().NotBeNull();
        items[0].Product!.Name.Value.Should().Be("First");
    }

    [Fact]
    public async Task Remove_ExistingAndMissing()
    {
        await _wishlist.AddAsync(new WishlistItem(_user.Id, _product1));

        (await _wishlist.RemoveAsync(_user.Id, _product1.Id)).Should().BeTrue();
        (await _wishlist.RemoveAsync(_user.Id, _product1.Id)).Should().BeFalse();
        (await _wishlist.CountByUserAsync(_user.Id)).Should().Be(0);
    }

    [Fact]
    public async Task OtherUsersWishlist_IsIsolated()
    {
        await _wishlist.AddAsync(new WishlistItem(_user.Id, _product1));

        (await _wishlist.GetProductIdsAsync(_user.Id + 100)).Should().BeEmpty();
    }

    [Fact]
    public async Task Users_GetUserNamesAsync_ReturnsDictionary()
    {
        var other = new User(UserName.Create("other"), PasswordHash.FromHash(new string('a', 60)));
        _context.Users.Add(other);
        _context.SaveChanges();

        var names = await _users.GetUserNamesAsync(new[] { _user.Id, other.Id, 9999 });

        names.Should().HaveCount(2);
        names[_user.Id].Should().Be("wisher");
        names[other.Id].Should().Be("other");
    }

    [Fact]
    public async Task Users_GetSecurityStampAsync()
    {
        (await _users.GetSecurityStampAsync(_user.Id)).Should().Be(_user.SecurityStamp);
        (await _users.GetSecurityStampAsync(9999)).Should().BeNull();
    }

    [Fact]
    public async Task Users_UpdateAsync_PersistsNewStamp()
    {
        var user = await _users.GetByIdAsync(_user.Id);
        user!.ChangeRole(Role.Admin);
        await _users.UpdateAsync(user);

        (await _users.GetSecurityStampAsync(_user.Id)).Should().Be(user.SecurityStamp);
    }
}
