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
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _handler = new ChangePasswordHandler(_userRepo.Object, _passwordHasher.Object, _jwtService.Object);

        _jwtService
            .Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("new.jwt.token");
    }

    private static User CreateUser()
    {
        var userName = UserName.Create("testuser");
        var hash = PasswordHash.FromHash(new string('a', 60));
        return new User(userName, hash, Role.User);
    }

    private static PasswordHash FakeHash() =>
        PasswordHash.FromHash(new string('b', 60));

    private void SetupValidUser(User user, string oldPassword = "oldpass1")
    {
        _userRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(oldPassword, It.IsAny<PasswordHash>()))
            .Returns(true);

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(FakeHash());
    }

    [Fact]
    public async Task Handle_ValidData_UpdatesPassword()
    {
        var user = CreateUser();
        SetupValidUser(user);

        await _handler.Handle(new ChangePasswordCommand(1, "oldpass1", "newpass123"), CancellationToken.None);

        _userRepo.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidData_HashesNewPassword()
    {
        SetupValidUser(CreateUser());

        await _handler.Handle(new ChangePasswordCommand(1, "oldpass1", "newpass123"), CancellationToken.None);

        _passwordHasher.Verify(h => h.Hash("newpass123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidData_RotatesSecurityStamp_AndReturnsFreshToken()
    {
        var user = CreateUser();
        var oldStamp = user.SecurityStamp;
        SetupValidUser(user);

        var result = await _handler.Handle(new ChangePasswordCommand(1, "oldpass1", "newpass123"), CancellationToken.None);

        user.SecurityStamp.Should().NotBe(oldStamp);
        result.Token.Should().Be("new.jwt.token");
        _jwtService.Verify(
            j => j.GenerateToken(user.Id, "testuser", "User", user.SecurityStamp),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFoundException()
    {
        _userRepo
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(
            new ChangePasswordCommand(999, "oldpass1", "newpass123"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_InvalidOldPassword_ThrowsDomainException()
    {
        var user = CreateUser();

        _userRepo
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("wrongold", It.IsAny<PasswordHash>()))
            .Returns(false);

        var act = async () => await _handler.Handle(
            new ChangePasswordCommand(1, "wrongold", "newpass123"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*старый пароль*");
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("short1")]
    [InlineData("onlyletters")]
    [InlineData("12345678")]
    public async Task Handle_WeakNewPassword_ThrowsDomainException(string newPassword)
    {
        // Раньше новый пароль не валидировался вовсе
        SetupValidUser(CreateUser());

        var act = async () => await _handler.Handle(
            new ChangePasswordCommand(1, "oldpass1", newPassword), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameAsOldPassword_ThrowsDomainException()
    {
        SetupValidUser(CreateUser(), oldPassword: "samepass1");

        var act = async () => await _handler.Handle(
            new ChangePasswordCommand(1, "samepass1", "samepass1"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*отличаться*");
    }
}
