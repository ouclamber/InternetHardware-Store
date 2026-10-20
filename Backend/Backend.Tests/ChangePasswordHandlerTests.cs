using Backend.Application.Users.Commands;
using Backend.Application.Users.Handlers;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _handler = new ChangePasswordHandler(_userRepo.Object, _passwordHasher.Object);
    }

    private static User CreateUser()
    {
        var userName = UserName.Create("testuser");
        var hash = PasswordHash.FromHash(new string('a', 60));
        return new User(userName, hash, Role.User);
    }

    private static PasswordHash FakeHash() =>
        PasswordHash.FromHash(new string('b', 60));

    [Fact]
    public async Task Handle_ValidData_UpdatesPassword()
    {
        var user = CreateUser();
        var command = new ChangePasswordCommand(1, "oldpass", "newpass123");

        _userRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("oldpass", It.IsAny<PasswordHash>()))
            .Returns(true);

        _passwordHasher
            .Setup(h => h.Hash("newpass123"))
            .Returns(FakeHash());

        await _handler.Handle(command, CancellationToken.None);

        _userRepo.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsDomainException()
    {
        var command = new ChangePasswordCommand(999, "oldpass", "newpass123");

        _userRepo
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_InvalidOldPassword_ThrowsDomainException()
    {
        var user = CreateUser();
        var command = new ChangePasswordCommand(1, "wrongold", "newpass123");

        _userRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("wrongold", It.IsAny<PasswordHash>()))
            .Returns(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_ValidData_HashesNewPassword()
    {
        var user = CreateUser();
        var command = new ChangePasswordCommand(1, "oldpass", "newpass123");

        _userRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("oldpass", It.IsAny<PasswordHash>()))
            .Returns(true);

        _passwordHasher
            .Setup(h => h.Hash("newpass123"))
            .Returns(FakeHash());

        await _handler.Handle(command, CancellationToken.None);

        _passwordHasher.Verify(h => h.Hash("newpass123"), Times.Once);
    }
}