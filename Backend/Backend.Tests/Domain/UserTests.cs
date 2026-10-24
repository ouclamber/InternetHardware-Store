using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class UserTests
{
    private static UserName ValidName(string name = "testuser") => UserName.Create(name);
    private static PasswordHash ValidHash() => PasswordHash.FromHash(new string('a', 60));

    [Fact]
    public void Constructor_ValidData_CreatesUser()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);

        user.UserName.Value.Should().Be("testuser");
        user.Role.Should().Be(Role.User);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_NullUserName_ThrowsDomainException()
    {
        var act = () => new User(null!, ValidHash(), Role.User);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_NullPasswordHash_ThrowsDomainException()
    {
        var act = () => new User(ValidName(), null!, Role.User);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_DefaultRole_IsUser()
    {
        var user = new User(ValidName(), ValidHash());
        user.Role.Should().Be(Role.User);
    }

    // === RENAME ===

    [Fact]
    public void Rename_NewName_UpdatesUserName()
    {
        var user = new User(ValidName("oldname"), ValidHash());
        var newName = UserName.Create("newname");

        user.Rename(newName);

        user.UserName.Value.Should().Be("newname");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Rename_SameName_DoesNotUpdate()
    {
        var user = new User(ValidName("somename"), ValidHash());
        var before = user.UpdatedAt;

        user.Rename(UserName.Create("somename"));

        user.UpdatedAt.Should().Be(before);
    }

    [Fact]
    public void Rename_NullName_ThrowsDomainException()
    {
        var user = new User(ValidName(), ValidHash());
        var act = () => user.Rename(null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangePassword_NewHash_UpdatesPassword()
    {
        var user = new User(ValidName(), ValidHash());
        var newHash = PasswordHash.FromHash(new string('b', 60));

        user.ChangePassword(newHash);

        user.PasswordHash.Value.Should().Be(newHash.Value);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ChangePassword_NullHash_ThrowsDomainException()
    {
        var user = new User(ValidName(), ValidHash());
        var act = () => user.ChangePassword(null!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeRole_ToAdmin_UpdatesRole()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);
        user.ChangeRole(Role.Admin);
        user.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public void ChangeRole_SameRole_DoesNotUpdate()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);
        var before = user.UpdatedAt;

        user.ChangeRole(Role.User);

        user.UpdatedAt.Should().Be(before);
    }

    [Fact]
    public void PromoteToAdmin_FromUser_UpdatesRole()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);
        user.PromoteToAdmin();
        user.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public void PromoteToAdmin_AlreadyAdmin_ThrowsDomainException()
    {
        var user = new User(ValidName(), ValidHash(), Role.Admin);
        var act = () => user.PromoteToAdmin();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void DemoteToUser_FromAdmin_UpdatesRole()
    {
        var user = new User(ValidName(), ValidHash(), Role.Admin);
        user.DemoteToUser();
        user.Role.Should().Be(Role.User);
    }

    [Fact]
    public void DemoteToUser_AlreadyUser_ThrowsDomainException()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);
        var act = () => user.DemoteToUser();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsAdmin_AdminRole_ReturnsTrue()
    {
        var user = new User(ValidName(), ValidHash(), Role.Admin);
        user.IsAdmin.Should().BeTrue();
        user.IsUser.Should().BeFalse();
    }

    [Fact]
    public void IsUser_UserRole_ReturnsTrue()
    {
        var user = new User(ValidName(), ValidHash(), Role.User);
        user.IsUser.Should().BeTrue();
        user.IsAdmin.Should().BeFalse();
    }
}