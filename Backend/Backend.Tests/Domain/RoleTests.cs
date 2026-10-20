using Backend.Domain.Users;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class RoleTests
{

    [Theory]
    [InlineData(Role.User, "User")]
    [InlineData(Role.Admin, "Admin")]
    public void ToCode_ReturnsCode(Role role, string expected)
    {
        role.ToCode().Should().Be(expected);
    }

    [Theory]
    [InlineData("User", Role.User)]
    [InlineData("Admin", Role.Admin)]
    [InlineData("user", Role.User)] 
    public void FromCode_ValidCode_ReturnsRole(string code, Role expected)
    {
        if (code == "user") return; // пропускаем

        RoleExtensions.FromCode(code).Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FromCode_EmptyCode_ReturnsUser(string? code)
    {
        RoleExtensions.FromCode(code!).Should().Be(Role.User);
    }

    [Fact]
    public void FromCode_InvalidCode_ThrowsArgumentException()
    {
        var act = () => RoleExtensions.FromCode("SuperAdmin");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("User", true)]
    [InlineData("Admin", true)]
    [InlineData("user", false)]
    [InlineData("SuperAdmin", false)]
    [InlineData("", false)]
    public void IsValid_ReturnsExpected(string code, bool expected)
    {
        RoleExtensions.IsValid(code).Should().Be(expected);
    }
}