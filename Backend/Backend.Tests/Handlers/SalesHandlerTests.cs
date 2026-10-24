using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Handlers;
using Backend.Application.Sales.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class SalesHandlerTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IEncryptionService> _encryption = new();

    private static Product CreateProduct(int id = 1, string name = "Test", decimal price = 1000)
    {
        var product = new Product(
            ProductName.Create(name),
            Money.Rub(price),
            brandId: 1,
            categoryId: 1,
            typeId: 1,
            stockQuantity: 100);


        var idProp = typeof(Product).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(product, id);

        return product;
    }

    private static CreateOrderCommand CreateOrderCommandInstance(int userId = 1)
    {
        return new CreateOrderCommand
        {
            UserId = userId,
            FirstName = "Иван",
            LastName = "Иванов",
            Email = "test@example.com",
            Phone = "+79001234567",
            Address = "ул. Ленина 1",
            City = "Москва",
            PostalCode = "101000",
            DeliveryMethod = "courier",
            PaymentMethod = "card",
            Comment = null
        };
    }

    [Fact]
    public async Task AddToCart_ValidData_AddsItem()
    {
        var handler = new AddToCartHandler(_cartRepo.Object, _productRepo.Object);
        var product = CreateProduct();
        var cart = new Cart(userId: 1);

        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cart);
        _cartRepo.Setup(r => r.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        await handler.Handle(new AddToCartCommand(1, 1, 2), CancellationToken.None);

        cart.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddToCart_ProductNotFound_ThrowsDomainException()
    {
        var handler = new AddToCartHandler(_cartRepo.Object, _productRepo.Object);
        _productRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Product?)null);

        var act = async () => await handler.Handle(new AddToCartCommand(1, 999, 2), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task AddToCart_NoCart_CreatesNew()
    {
        var handler = new AddToCartHandler(_cartRepo.Object, _productRepo.Object);
        var product = CreateProduct();

        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(product);
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Cart?)null);
        _cartRepo.Setup(r => r.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);
        _cartRepo.Setup(r => r.UpdateAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        await handler.Handle(new AddToCartCommand(1, 1, 2), CancellationToken.None);

        _cartRepo.Verify(r => r.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCart_EmptyCart_ReturnsEmptyDto()
    {
        var handler = new GetCartHandler(_cartRepo.Object);
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Cart?)null);

        var result = await handler.Handle(new GetCartQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalQuantity.Should().Be(0);
        result.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task GetCart_WithItems_ReturnsDto()
    {
        var handler = new GetCartHandler(_cartRepo.Object);
        var cart = new Cart(userId: 1);
        cart.AddItem(CreateProduct(), 2);

        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cart);

        var result = await handler.Handle(new GetCartQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalQuantity.Should().Be(2);
    }

    [Fact]
    public async Task ClearCart_ValidUserId_CallsRepository()
    {
        var handler = new ClearCartHandler(_cartRepo.Object);
        _cartRepo.Setup(r => r.ClearByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        await handler.Handle(new ClearCartCommand(1), CancellationToken.None);

        _cartRepo.Verify(r => r.ClearByUserIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrder_EmptyCart_ThrowsDomainException()
    {
        var handler = new CreateOrderHandler(_cartRepo.Object, _orderRepo.Object);
        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Cart?)null);

        var command = CreateOrderCommandInstance(1);
        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task CreateOrder_ValidCart_CreatesOrder()
    {
        var handler = new CreateOrderHandler(_cartRepo.Object, _orderRepo.Object);
        var cart = new Cart(userId: 1);
        var product = CreateProduct(id: 1, name: "Test Product", price: 1000);
        cart.AddItem(product, 2);

        _cartRepo.Setup(r => r.GetByUserIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(cart);
        _orderRepo.Setup(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        var command = CreateOrderCommandInstance(1);
        await handler.Handle(command, CancellationToken.None);

        // Заказ и очистка корзины — одной атомарной операцией
        _orderRepo.Verify(r => r.AddAndClearCartAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _cartRepo.Verify(r => r.ClearByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}