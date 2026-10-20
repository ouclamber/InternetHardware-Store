using Backend.Application.Users.Commands;
using Backend.Application.Users.Handlers;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Handlers;

public class SignUpHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly SignUpHandler _handler;

    public SignUpHandlerTests()
    {
        _handler = new SignUpHandler(_userRepo.Object, _passwordHasher.Object);
    }

    private static PasswordHash FakeHash() =>
        PasswordHash.FromHash(new string('a', 60));

    private static SignUpCommand ValidCommand(string userName = "newuser") => new()
    {
        UserName = userName,
        Password = "password123",
        ConfirmPassword = "password123"
    };

    private void SetupHappyPath()
    {
        _userRepo
            .Setup(r => r.ExistsAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns(FakeHash());

        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_ValidData_CreatesUser()
    {
        SetupHappyPath();

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Should().BeGreaterThanOrEqualTo(0);
        _userRepo.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PasswordsDoNotMatch_ThrowsDomainException()
    {
        var command = ValidCommand();
        command.ConfirmPassword = "otherpassword";

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short1")]
    public async Task Handle_WeakPassword_ThrowsDomainException(string password)
    {
        var command = ValidCommand();
        command.Password = password;
        command.ConfirmPassword = password;

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _userRepo.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserAlreadyExists_ThrowsDomainException()
    {
        _userRepo
            .Setup(r => r.ExistsAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _handler.Handle(ValidCommand("existing"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_ValidData_HashesPassword()
    {
        SetupHappyPath();

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _passwordHasher.Verify(h => h.Hash("password123"), Times.Once);
    }

    [Fact]
    public async Task Handle_Always_CreatesUserWithUserRole()
    {
        // Регрессионный тест: через регистрацию нельзя получить права администратора.
        SetupHappyPath();

        User? capturedUser = null;
        _userRepo
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(Role.User);
    }
}
