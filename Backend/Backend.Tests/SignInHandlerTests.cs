using Backend.Application.Users.Commands;
using Backend.Application.Users.Handlers;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class SignInHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly SignInHandler _handler;

    public SignInHandlerTests()
    {
        _handler = new SignInHandler(
            _userRepo.Object,
            _passwordHasher.Object,
            _jwtService.Object);
    }

    private static User CreateUser(string name = "testuser", Role role = Role.User)
    {
        var userName = UserName.Create(name);
        var hash = PasswordHash.FromHash(new string('a', 60)); // фейковый BCrypt-хеш
        return new User(userName, hash, role);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResult()
    {
        // Arrange
        var user = CreateUser("testuser", Role.User);
        var command = new SignInCommand
        {
            UserName = "testuser",
            Password = "password123",
            Role = "User"
        };

        _userRepo
            .Setup(r => r.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("password123", It.IsAny<PasswordHash>()))
            .Returns(true);

        _jwtService
            .Setup(j => j.GenerateToken(user.Id, "testuser", "User"))
            .Returns("fake.jwt.token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("fake.jwt.token");
        result.UserName.Should().Be("testuser");
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_EmptyUserName_ThrowsDomainException()
    {
        var command = new SignInCommand
        {
            UserName = "",
            Password = "password123",
            Role = "User"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_EmptyPassword_ThrowsDomainException()
    {
        var command = new SignInCommand
        {
            UserName = "testuser",
            Password = "",
            Role = "User"
        };

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsDomainException()
    {
        var command = new SignInCommand
        {
            UserName = "nonexistent",
            Password = "password123",
            Role = "User"
        };

        _userRepo
            .Setup(r => r.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsDomainException()
    {
        var user = CreateUser("testuser", Role.User);
        var command = new SignInCommand
        {
            UserName = "testuser",
            Password = "wrongpassword",
            Role = "User"
        };

        _userRepo
            .Setup(r => r.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("wrongpassword", It.IsAny<PasswordHash>()))
            .Returns(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_WrongRole_ThrowsDomainException()
    {
        var user = CreateUser("testuser", Role.User); // роль в БД: User
        var command = new SignInCommand
        {
            UserName = "testuser",
            Password = "password123",
            Role = "Admin" // пытаемся войти как Admin
        };

        _userRepo
            .Setup(r => r.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify("password123", It.IsAny<PasswordHash>()))
            .Returns(true);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}