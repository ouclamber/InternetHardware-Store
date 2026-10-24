using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Handlers;
using Backend.Application.Sales.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using Backend.Tests.TestHelpers;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Sales.Handlers;

/// <summary>
/// Оформление и отмена заказа: склад, контакты, права доступа.
/// </summary>
public class OrderStockHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private static CreateOrderCommand Command(int userId = 1) => new()
    {
        UserId = userId,
        FirstName = "Иван",
        LastName = "Иванов",
        Email = "Ivan@Example.com",
        Phone = "+7 900 123-45-67",
        Address = "ул. Ленина 1",
        City = "Москва",
        PostalCode = "101000",
        DeliveryMethod = "courier",
        PaymentMethod = "card"
    };

    private void SetupCart(Cart cart)
    {
        _cartRepo.Setup(r => r.GetByUserIdAsync(cart.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _orderRepo.Setup(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);
    }

    private CreateOrderHandler CreateHandler() => new(_cartRepo.Object, _orderRepo.Object);

    // ============================================================
    // === Оформление заказа ===
    // ============================================================

    [Fact]
    public async Task CreateOrder_ReservesStock_AndMarksOrder()
    {
        var cart = new Cart(1);
        var p1 = TestData.Product(id: 1, price: 1000, stock: 5);
        var p2 = TestData.Product(id: 2, price: 300, stock: 1);
        cart.AddItem(p1, 2);
        cart.AddItem(p2, 1);

        Order? saved = null;
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _orderRepo.Setup(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                  .Callback<Order, CancellationToken>((o, _) => saved = o)
                  .Returns(Task.CompletedTask);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        p1.StockQuantity.Should().Be(3);
        p2.StockQuantity.Should().Be(0);
        saved.Should().NotBeNull();
        saved!.StockReserved.Should().BeTrue();
        saved.TotalAmount.Amount.Should().Be(2300);
    }

    [Fact]
    public async Task CreateOrder_SavesCustomerContact()
    {
        var cart = new Cart(1);
        cart.AddItem(TestData.Product(stock: 5), 1);

        Order? saved = null;
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _orderRepo.Setup(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                  .Callback<Order, CancellationToken>((o, _) => saved = o)
                  .Returns(Task.CompletedTask);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        // Раньше имя, email и телефон из формы оформления просто терялись
        saved!.Contact.Should().NotBeNull();
        saved.Contact!.FullName.Should().Be("Иван Иванов");
        saved.Contact.Email.Should().Be("ivan@example.com");
        saved.Contact.Phone.Should().Be("+79001234567");
    }

    [Fact]
    public async Task CreateOrder_ProductDeactivatedAfterAddingToCart_Throws_AndSavesNothing()
    {
        var cart = new Cart(1);
        var product = TestData.Product(stock: 5);
        cart.AddItem(product, 1);
        product.Deactivate();
        SetupCart(cart);

        var act = async () => await CreateHandler().Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*недоступен*");
        _orderRepo.Verify(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_StockSoldOutMeanwhile_Throws_AndDoesNotTouchOtherProducts()
    {
        var cart = new Cart(1);
        var ok = TestData.Product(id: 1, stock: 5);
        var soldOut = TestData.Product(id: 2, stock: 3);
        cart.AddItem(ok, 2);
        cart.AddItem(soldOut, 3);
        soldOut.SetStock(1);   // пока товар лежал в корзине, его раскупили
        SetupCart(cart);

        var act = async () => await CreateHandler().Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*доступно 1*");
        ok.StockQuantity.Should().Be(5);   // проверки — до любых списаний
        _orderRepo.Verify(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_InvalidEmail_Throws_BeforeReservingStock()
    {
        var cart = new Cart(1);
        var product = TestData.Product(stock: 5);
        cart.AddItem(product, 2);
        SetupCart(cart);

        var command = Command();
        command.Email = "broken";

        var act = async () => await CreateHandler().Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*email*");
        product.StockQuantity.Should().Be(5);
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_Throws()
    {
        SetupCart(new Cart(1));

        var act = async () => await CreateHandler().Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*пуста*");
    }

    [Fact]
    public async Task CreateOrder_UsesCurrentPrice()
    {
        var cart = new Cart(1);
        var product = TestData.Product(price: 1000, stock: 5);
        cart.AddItem(product, 1);
        product.UpdatePrice(Money.Rub(1200));

        Order? saved = null;
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _orderRepo.Setup(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                  .Callback<Order, CancellationToken>((o, _) => saved = o)
                  .Returns(Task.CompletedTask);

        await CreateHandler().Handle(Command(), CancellationToken.None);

        saved!.TotalAmount.Amount.Should().Be(1200);
    }

    // ============================================================
    // === Отмена заказа ===
    // ============================================================

    private static Order ReservedOrder(int userId, int productId, int qty)
    {
        var order = new Order(
            userId,
            Address.Create("ул. Ленина 1", "Москва"),
            new[] { (productId, "Товар", qty, Money.Rub(100)) });
        order.MarkStockReserved();
        return order;
    }

    private CancelOrderHandler CancelHandler() => new(_orderRepo.Object, _productRepo.Object, _unitOfWork.Object);

    [Fact]
    public async Task CancelOrder_Owner_CancelsAndReturnsStock()
    {
        var order = ReservedOrder(userId: 1, productId: 7, qty: 3);
        var product = TestData.Product(id: 7, stock: 0);

        _orderRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new[] { product });

        await CancelHandler().Handle(new CancelOrderCommand(10, 1), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        product.StockQuantity.Should().Be(3);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_NotOwner_ThrowsForbidden_AndSavesNothing()
    {
        var order = ReservedOrder(userId: 1, productId: 7, qty: 1);
        _orderRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var act = async () => await CancelHandler().Handle(new CancelOrderCommand(10, 2), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        order.Status.Should().Be(OrderStatus.Pending);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_NotFound_ThrowsNotFound()
    {
        _orderRepo.Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        var act = async () => await CancelHandler().Handle(new CancelOrderCommand(404, 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelOrder_Shipped_Throws_AndKeepsStock()
    {
        var order = ReservedOrder(userId: 1, productId: 7, qty: 1);
        order.MarkAsPaid();
        order.Ship();
        _orderRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var act = async () => await CancelHandler().Handle(new CancelOrderCommand(10, 1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _productRepo.Verify(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_ProductPhysicallyDeleted_StillCancels()
    {
        var order = ReservedOrder(userId: 1, productId: 7, qty: 1);
        _orderRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _productRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Array.Empty<Backend.Domain.Catalog.Entities.Product>());

        await CancelHandler().Handle(new CancelOrderCommand(10, 1), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    // ============================================================
    // === Корзина: флаг доступности ===
    // ============================================================

    [Fact]
    public async Task GetCart_ReportsAvailability()
    {
        var cart = new Cart(1);
        var available = TestData.Product(id: 1, stock: 5);
        var soldOut = TestData.Product(id: 2, stock: 2);
        cart.AddItem(available, 1);
        cart.AddItem(soldOut, 2);
        soldOut.SetStock(1);

        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(cart);

        var dto = await new GetCartHandler(_cartRepo.Object).Handle(new GetCartQuery(1), CancellationToken.None);

        dto!.Items.Single(i => i.ProductId == 1).IsAvailable.Should().BeTrue();
        var item2 = dto.Items.Single(i => i.ProductId == 2);
        item2.IsAvailable.Should().BeFalse();
        item2.StockQuantity.Should().Be(1);
    }
}
