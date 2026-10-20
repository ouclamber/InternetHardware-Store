using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Services;
using FluentAssertions;

namespace Backend.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ValidPassword_ReturnsPasswordHash()
    {
        var hash = _hasher.Hash("password123");
        hash.Should().NotBeNull();
        hash.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ReturnsDifferentHashes()
    {
        var hash1 = _hasher.Hash("password123");
        var hash2 = _hasher.Hash("password123");
        hash1.Value.Should().NotBe(hash2.Value); // BCrypt использует salt
    }

    [Fact]
    public void Hash_EmptyPassword_ThrowsArgumentException()
    {
        var act = () => _hasher.Hash("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Hash_WhitespacePassword_ThrowsArgumentException()
    {
        var act = () => _hasher.Hash("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("password123");
        _hasher.Verify("password123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("password123");
        _hasher.Verify("wrongpassword", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("password123");
        _hasher.Verify("", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_NullHash_ReturnsFalse()
    {
        _hasher.Verify("password123", null!).Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHash_ReturnsFalse()
    {
        var invalidHash = PasswordHash.FromHash(new string('a', 60));
        _hasher.Verify("password123", invalidHash).Should().BeFalse();
    }

    [Fact]
    public void HashAndVerify_RealWorldScenario_Works()
    {
        var password = "SuperSecure123!@#";
        var hash = _hasher.Hash(password);

        _hasher.Verify(password, hash).Should().BeTrue();
        _hasher.Verify(password + "x", hash).Should().BeFalse();
    }
}