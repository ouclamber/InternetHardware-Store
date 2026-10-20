using Backend.Domain.Shared;
using Backend.Domain.Users.ValueObjects;

namespace Backend.Domain.Users;

public class User : Entity, IAggregateRoot
{
    public UserName UserName { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public Role Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Для EF Core
    protected User() { }

    public User(UserName userName, PasswordHash passwordHash, Role role = Role.User)
    {
        if (userName == null)
            throw new DomainException("Имя пользователя обязательно");

        if (passwordHash == null)
            throw new DomainException("Пароль обязателен");

        UserName = userName;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(UserName newUserName)
    {
        if (newUserName == null)
            throw new DomainException("Новое имя пользователя обязательно");

        if (newUserName.Value == UserName.Value)
            return;   // ничего не изменилось

        UserName = newUserName;
        MarkAsUpdated();
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        if (newPasswordHash == null)
            throw new DomainException("Новый хеш пароля обязателен");

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void ChangeRole(Role newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
        MarkAsUpdated();
    }

    public void PromoteToAdmin()
    {
        if (Role == Role.Admin)
            throw new DomainException("Пользователь уже является администратором");

        Role = Role.Admin;
        MarkAsUpdated();
    }

    public void DemoteToUser()
    {
        if (Role == Role.User)
            throw new DomainException("Пользователь уже является обычным пользователем");

        Role = Role.User;
        MarkAsUpdated();
    }

    public bool IsAdmin => Role == Role.Admin;
    public bool IsUser => Role == Role.User;

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}