using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Backend.Tests.Services;

public class AdminSeederTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();

    private static IConfiguration Config(string? userName = null, string? password = null)
    {
        var dict = new Dictionary<string, string?>();
        if (userName != null) dict["Admin:UserName"] = userName;
        if (password != null) dict["Admin:Password"] = password;

        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static PasswordHash FakeHash() =>
        PasswordHash.FromHash(new string('a', 60));

    private Task<bool> Run(IConfiguration config) =>
        AdminSeeder.EnsureAdminAsync(
            _users.Object, _hasher.Object, config, NullLogger.Instance);

    [Fact]
    public async Task NoConfig_DoesNothing()
    {
        var created = await Run(Config());

        created.Should().BeFalse();
        _users.Verify(
            u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShortPassword_Throws()
    {
        var act = () => Run(Config("admin", "short"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AdminAlreadyExists_DoesNothing()
    {
        _users
            .Setup(u => u.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var created = await Run(Config("admin", "strong-password"));

        created.Should().BeFalse();
        _users.Verify(
            u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NoAdmin_CreatesAdminWithAdminRole()
    {
        _users
            .Setup(u => u.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _users
            .Setup(u => u.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _hasher.Setup(h => h.Hash("strong-password")).Returns(FakeHash());

        User? captured = null;
        _users
            .Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        var created = await Run(Config("admin", "strong-password"));

        created.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Role.Should().Be(Role.Admin);
        captured.UserName.Value.Should().Be("admin");
    }

    [Fact]
    public async Task ExistingUserWithSameName_IsPromoted()
    {
        var existing = new User(UserName.Create("admin"), FakeHash(), Role.User);

        _users
            .Setup(u => u.CountByRoleAsync(Role.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _users
            .Setup(u => u.GetByUserNameAsync(It.IsAny<UserName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _users
            .Setup(u => u.UpdateAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var created = await Run(Config("admin", "strong-password"));

        created.Should().BeTrue();
        existing.Role.Should().Be(Role.Admin);
        _users.Verify(
            u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
