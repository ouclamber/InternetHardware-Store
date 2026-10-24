using Backend.Domain.Shared;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class UserNameTests
{

    [Theory]
    [InlineData("user")]
    [InlineData("user123")]
    [InlineData("user_name")]
    [InlineData("user-name")]
    [InlineData("Admin")]
    [InlineData("abcdefghij")]
    public void Create_ValidName_CreatesUserName(string name)
    {
        var userName = UserName.Create(name);
        userName.Value.Should().Be(name);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var userName = UserName.Create("  testuser  ");
        userName.Value.Should().Be("testuser");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyName_ThrowsDomainException(string? name)
    {
        var act = () => UserName.Create(name!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooShort_ThrowsDomainException()
    {
        var act = () => UserName.Create("ab");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLong_ThrowsDomainException()
    {
        var act = () => UserName.Create(new string('a', 51));
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user name")]
    [InlineData("user!name")]
    [InlineData("user#name")]
    public void Create_InvalidCharacters_ThrowsDomainException(string name)
    {
        var act = () => UserName.Create(name);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_StartsWithDigit_ThrowsDomainException()
    {
        var act = () => UserName.Create("123user");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = UserName.Create("testuser");
        var b = UserName.Create("testuser");
        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = UserName.Create("user1");
        var b = UserName.Create("user2");
        a.Should().NotBe(b);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        var userName = UserName.Create("testuser");
        string value = userName;
        value.Should().Be("testuser");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var userName = UserName.Create("testuser");
        userName.ToString().Should().Be("testuser");
    }
}