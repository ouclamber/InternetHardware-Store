using Backend.Application.Admin.Commands;
using Backend.Application.Admin.Handlers;
using Backend.Application.Admin.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace Backend.Tests.Handlers;

public class AdminHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IReviewRepository> _reviewRepo = new();

    private static User CreateUser(int id = 1, string name = "testuser", Role role = Role.User)
    {
        var user = new User(
            UserName.Create(name),
            PasswordHash.FromHash(new string('a', 60)),
            role);

        var idProp = typeof(User).GetProperty("Id",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        idProp?.SetValue(user, id);

        return user;
    }

    [Fact]
    public async Task GetUsers_ReturnsList()
    {
        var handler = new GetUsersHandler(_userRepo.Object);
        var users = new List<User> { CreateUser(1), CreateUser(2, "user2") };
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(users);

        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUsers_Empty_ReturnsEmptyList()
    {
        var handler = new GetUsersHandler(_userRepo.Object);
        _userRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<User>());

        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateUserRole_ValidData_Updates()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);
        var user = CreateUser(1, "user1", Role.User);
        _userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _userRepo.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        // (userId, role)
        await handler.Handle(new UpdateUserRoleCommand(1, "Admin"), CancellationToken.None);

        user.Role.Should().Be(Role.Admin);
        _userRepo.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRole_NotFound_ThrowsDomainException()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);
        _userRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

        var act = async () => await handler.Handle(new UpdateUserRoleCommand(999, "Admin"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task DeleteUser_ValidData_Deletes()
    {
        var handler = new DeleteUserHandler(_userRepo.Object);
        var user = CreateUser(1);
        _userRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _userRepo.Setup(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        // (userId)
        await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        _userRepo.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_NotFound_ThrowsDomainException()
    {
        var handler = new DeleteUserHandler(_userRepo.Object);
        _userRepo.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((User?)null);

        var act = async () => await handler.Handle(new DeleteUserCommand(999), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetAllOrders_ReturnsList()
    {
        var handler = new GetAllOrdersHandler(_orderRepo.Object, _userRepo.Object);
        _orderRepo.Setup(r => r.GetPagedAsync(null, 1, 20, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(PagedResult<Order>.Empty(1, 20));

        var result = await handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAdminStats_ReturnsStats()
    {
        var handler = new GetAdminStatsHandler(
            _userRepo.Object,
            _productRepo.Object,
            _orderRepo.Object,
            _reviewRepo.Object);

        _userRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(10);
        _userRepo.Setup(r => r.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _productRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(50);
        _productRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(45);
        _orderRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(20);
        _orderRepo.Setup(r => r.SumRevenueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(100000);

        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(10);
        result.TotalAdmins.Should().Be(2);
        result.TotalRegularUsers.Should().Be(8);
        result.TotalProducts.Should().Be(50);
        result.ActiveProducts.Should().Be(45);
        result.InactiveProducts.Should().Be(5);
        result.TotalOrders.Should().Be(20);
        result.TotalRevenue.Should().Be(100000);
    }

    [Fact]
    public async Task GetAdminStats_EmptyDb_ReturnsZeros()
    {
        var handler = new GetAdminStatsHandler(
            _userRepo.Object,
            _productRepo.Object,
            _orderRepo.Object,
            _reviewRepo.Object);

        _userRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _userRepo.Setup(r => r.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _productRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _productRepo.Setup(r => r.CountActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _orderRepo.Setup(r => r.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _orderRepo.Setup(r => r.SumRevenueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalUsers.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
    }

    // ============================================================
    // === Защита ролей и пользователей ===
    // ============================================================

    private static User CreateUserWithId(int id, string name, Role role)
    {
        var user = new User(UserName.Create(name), PasswordHash.FromHash(new string('a', 60)), role);
        typeof(User).GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(user, id);
        return user;
    }

    [Fact]
    public async Task UpdateUserRole_OwnRole_ThrowsForbidden()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);

        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand(5, "User", requestingUserId: 5), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRole_DemoteLastAdmin_ThrowsDomainException()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);
        var admin = CreateUserWithId(2, "admin2", Role.Admin);

        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _userRepo.Setup(r => r.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var act = async () => await handler.Handle(
            new UpdateUserRoleCommand(2, "User", requestingUserId: 1), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*последнего администратора*");
        admin.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task UpdateUserRole_DemoteAdmin_WhenOtherAdminsExist_Succeeds_AndRotatesStamp()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);
        var admin = CreateUserWithId(2, "admin2", Role.Admin);
        var stamp = admin.SecurityStamp;

        _userRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(admin);
        _userRepo.Setup(r => r.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        await handler.Handle(new UpdateUserRoleCommand(2, "User", requestingUserId: 1), CancellationToken.None);

        admin.Role.Should().Be(Role.User);
        admin.SecurityStamp.Should().NotBe(stamp);   // старый токен с ролью Admin больше не действует
        _userRepo.Verify(r => r.UpdateAsync(admin, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRole_UserNotFound_ThrowsNotFound()
    {
        var handler = new UpdateUserRoleHandler(_userRepo.Object);
        _userRepo.Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var act = async () => await handler.Handle(new UpdateUserRoleCommand(42, "Admin", 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteUser_Self_ThrowsForbidden()
    {
        var handler = new DeleteUserHandler(_userRepo.Object);

        var act = async () => await handler.Handle(new DeleteUserCommand(3, requestingUserId: 3), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _userRepo.Verify(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ============================================================
    // === Заказы в админке: пагинация и имена без N+1 ===
    // ============================================================

    private static Order CreateOrder(int userId)
    {
        return new Order(
            userId,
            Address.Create("ул. Ленина 1", "Москва"),
            new[] { (1, "Товар", 2, Money.Rub(1000)) },
            contact: CustomerContact.Create("Иван", "Иванов", "ivan@example.com", "+79001234567"));
    }

    [Fact]
    public async Task GetAllOrders_LoadsUserNamesWithSingleQuery()
    {
        var handler = new GetAllOrdersHandler(_orderRepo.Object, _userRepo.Object);
        var orders = new List<Order> { CreateOrder(1), CreateOrder(2), CreateOrder(1) };

        _orderRepo.Setup(r => r.GetPagedAsync(null, 1, 20, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new PagedResult<Order>(orders, 3, 1, 20));
        _userRepo.Setup(r => r.GetUserNamesAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Dictionary<int, string> { [1] = "alice", [2] = "bob" });

        var result = await handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.Items[0].UserName.Should().Be("alice");
        result.Items[1].UserName.Should().Be("bob");
        result.Items[0].CustomerName.Should().Be("Иван Иванов");
        result.Items[0].TotalAmount.Should().Be(2000);

        _userRepo.Verify(r => r.GetUserNamesAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllOrders_StatusFilterAndPaging_PassedToRepository()
    {
        var handler = new GetAllOrdersHandler(_orderRepo.Object, _userRepo.Object);
        _orderRepo.Setup(r => r.GetPagedAsync(OrderStatus.Paid, 3, 10, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(PagedResult<Order>.Empty(3, 10));

        var result = await handler.Handle(
            new GetAllOrdersQuery { Status = "PAID", Page = 3, PageSize = 10 }, CancellationToken.None);

        result.Page.Should().Be(3);
        _orderRepo.Verify(r => r.GetPagedAsync(OrderStatus.Paid, 3, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrders_PageSizeIsCapped()
    {
        var handler = new GetAllOrdersHandler(_orderRepo.Object, _userRepo.Object);
        _orderRepo.Setup(r => r.GetPagedAsync(null, 1, Paging.MaxPageSize, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(PagedResult<Order>.Empty(1, Paging.MaxPageSize));

        await handler.Handle(new GetAllOrdersQuery { PageSize = 100_000 }, CancellationToken.None);

        _orderRepo.Verify(r => r.GetPagedAsync(null, 1, Paging.MaxPageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllOrders_UnknownStatus_ThrowsDomainException()
    {
        var handler = new GetAllOrdersHandler(_orderRepo.Object, _userRepo.Object);

        var act = async () => await handler.Handle(new GetAllOrdersQuery { Status = "lost" }, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task GetAdminStats_IncludesOutOfStockAndPendingReviews()
    {
        var handler = new GetAdminStatsHandler(_userRepo.Object, _productRepo.Object, _orderRepo.Object, _reviewRepo.Object);

        _productRepo.Setup(r => r.CountOutOfStockAsync(It.IsAny<CancellationToken>())).ReturnsAsync(4);
        _reviewRepo.Setup(r => r.CountPendingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(7);

        var result = await handler.Handle(new GetAdminStatsQuery(), CancellationToken.None);

        result.OutOfStockProducts.Should().Be(4);
        result.PendingReviews.Should().Be(7);
    }
}
