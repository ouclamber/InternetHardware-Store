using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class UserSecurityStampTests
{
    private static User CreateUser(Role role = Role.User) =>
        new(UserName.Create("stampuser"), PasswordHash.FromHash(new string('a', 60)), role);

    [Fact]
    public void NewUser_HasRandomStamp()
    {
        var a = CreateUser();
        var b = CreateUser();

        a.SecurityStamp.Should().NotBeNullOrWhiteSpace();
        a.SecurityStamp.Should().NotBe(b.SecurityStamp);
    }

    [Fact]
    public void ChangePassword_RotatesStamp()
    {
        var user = CreateUser();
        var before = user.SecurityStamp;

        user.ChangePassword(PasswordHash.FromHash(new string('b', 60)));

        user.SecurityStamp.Should().NotBe(before);
    }

    [Fact]
    public void ChangeRole_RotatesStamp()
    {
        var user = CreateUser();
        var before = user.SecurityStamp;

        user.ChangeRole(Role.Admin);

        user.SecurityStamp.Should().NotBe(before);
    }

    [Fact]
    public void ChangeRole_SameRole_KeepsStamp()
    {
        var user = CreateUser();
        var before = user.SecurityStamp;

        user.ChangeRole(Role.User);

        user.SecurityStamp.Should().Be(before);
    }

    [Fact]
    public void PromoteAndDemote_RotateStamp()
    {
        var user = CreateUser();
        var s0 = user.SecurityStamp;

        user.PromoteToAdmin();
        var s1 = user.SecurityStamp;
        user.DemoteToUser();

        s1.Should().NotBe(s0);
        user.SecurityStamp.Should().NotBe(s1);
    }

    [Fact]
    public void Rename_KeepsStamp()
    {
        var user = CreateUser();
        var before = user.SecurityStamp;

        user.Rename(UserName.Create("renamed"));

        user.SecurityStamp.Should().Be(before);
    }
}
