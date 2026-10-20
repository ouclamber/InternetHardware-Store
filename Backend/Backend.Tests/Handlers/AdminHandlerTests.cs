using Backend.Application.Admin.Commands;
using Backend.Application.Admin.Handlers;
using Backend.Application.Admin.Queries;
using Backend.Domain.Catalog.Repositories;
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
        _orderRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new List<Backend.Domain.Sales.OrderAggregate.Order>());

        var result = await handler.Handle(new GetAllOrdersQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAdminStats_ReturnsStats()
    {
        var handler = new GetAdminStatsHandler(
            _userRepo.Object,
            _productRepo.Object,
            _orderRepo.Object);

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
            _orderRepo.Object);

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
}