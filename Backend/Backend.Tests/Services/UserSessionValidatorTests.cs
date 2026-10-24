using Backend.Domain.Users.Repositories;
using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Backend.Tests.Services;

public class UserSessionValidatorTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    private UserSessionValidator Create() => new(_users.Object, _cache);

    [Fact]
    public async Task MatchingStamp_IsValid()
    {
        _users.Setup(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync("stamp-A");

        (await Create().IsValidAsync(1, "stamp-A")).Should().BeTrue();
    }

    [Fact]
    public async Task DifferentStamp_IsInvalid()
    {
        // Пароль/роль сменились → у токена старая метка
        _users.Setup(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync("stamp-B");

        (await Create().IsValidAsync(1, "stamp-A")).Should().BeFalse();
    }

    [Fact]
    public async Task DeletedUser_IsInvalid()
    {
        _users.Setup(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        (await Create().IsValidAsync(1, "stamp-A")).Should().BeFalse();
    }

    [Theory]
    [InlineData(0, "stamp")]
    [InlineData(1, null)]
    [InlineData(1, "")]
    public async Task MissingData_IsInvalid_WithoutDbCall(int userId, string? stamp)
    {
        (await Create().IsValidAsync(userId, stamp)).Should().BeFalse();

        _users.Verify(r => r.GetSecurityStampAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Result_IsCached()
    {
        _users.Setup(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync("stamp-A");
        var validator = Create();

        await validator.IsValidAsync(1, "stamp-A");
        await validator.IsValidAsync(1, "stamp-A");
        await validator.IsValidAsync(1, "other");

        _users.Verify(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CacheEviction_PicksUpNewStamp()
    {
        var stamp = "stamp-A";
        _users.Setup(r => r.GetSecurityStampAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(() => stamp);
        var validator = Create();

        (await validator.IsValidAsync(1, "stamp-A")).Should().BeTrue();

        stamp = "stamp-B";
        _cache.Remove(UserSessionValidator.CacheKey(1));   // так делает UserRepository при UpdateAsync

        (await validator.IsValidAsync(1, "stamp-A")).Should().BeFalse();
    }
}
