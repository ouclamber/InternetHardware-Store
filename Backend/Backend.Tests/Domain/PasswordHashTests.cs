using Backend.Domain.Shared;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class PasswordHashTests
{

    [Fact]
    public void FromHash_ValidHash_CreatesPasswordHash()
    {
        var hash = new string('a', 60);
        var passwordHash = PasswordHash.FromHash(hash);
        passwordHash.Value.Should().Be(hash);
    }

    [Fact]
    public void FromHash_TrimsWhitespace()
    {
        var hash = new string('a', 60);
        var passwordHash = PasswordHash.FromHash($"  {hash}  ");
        passwordHash.Value.Should().Be(hash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromHash_EmptyHash_ThrowsDomainException(string? hash)
    {
        var act = () => PasswordHash.FromHash(hash!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FromHash_TooShort_ThrowsDomainException()
    {
        var act = () => PasswordHash.FromHash(new string('a', 19));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FromHash_TooLong_ThrowsDomainException()
    {
        var act = () => PasswordHash.FromHash(new string('a', 201));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameHash_ReturnsTrue()
    {
        var hash = new string('a', 60);
        var a = PasswordHash.FromHash(hash);
        var b = PasswordHash.FromHash(hash);
        a.Should().Be(b);
    }

    [Fact]
    public void ToString_ReturnsMasked()
    {
        var hash = PasswordHash.FromHash(new string('a', 60));
        hash.ToString().Should().Be("[HASH]");
    }
}