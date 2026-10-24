using Backend.Application.Common;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Common;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("password1")]
    [InlineData("Пароль2024")]
    [InlineData("a1b2c3d4")]
    public void Validate_StrongEnough_DoesNotThrow(string password)
    {
        var act = () => PasswordPolicy.Validate(password);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc1")]
    [InlineData("onlyletters")]
    [InlineData("1234567890")]
    public void Validate_Weak_Throws(string? password)
    {
        var act = () => PasswordPolicy.Validate(password);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_LongerThanBcryptLimit_Throws()
    {
        // 73 байта: BCrypt молча обрезал бы всё после 72-го
        var act = () => PasswordPolicy.Validate(new string('a', 72) + "1");

        act.Should().Throw<DomainException>().WithMessage("*72*");
    }

    [Fact]
    public void Validate_CyrillicCountsInBytes()
    {
        // 37 кириллических символов = 74 байта в UTF-8
        var act = () => PasswordPolicy.Validate(new string('я', 36) + "1");

        act.Should().Throw<DomainException>();
    }
}
