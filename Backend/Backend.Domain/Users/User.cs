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

    /// <summary>
    /// Случайная метка безопасности. Попадает в JWT и меняется при смене пароля
    /// или роли — после этого все ранее выданные токены перестают приниматься.
    /// </summary>
    public string SecurityStamp { get; private set; } = string.Empty;

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
        SecurityStamp = NewStamp();
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
        RotateSecurityStamp();
        MarkAsUpdated();
    }

    public void ChangeRole(Role newRole)
    {
        if (Role == newRole)
            return;

        Role = newRole;
        RotateSecurityStamp();
        MarkAsUpdated();
    }

    public void PromoteToAdmin()
    {
        if (Role == Role.Admin)
            throw new DomainException("Пользователь уже является администратором");

        Role = Role.Admin;
        RotateSecurityStamp();
        MarkAsUpdated();
    }

    public void DemoteToUser()
    {
        if (Role == Role.User)
            throw new DomainException("Пользователь уже является обычным пользователем");

        Role = Role.User;
        RotateSecurityStamp();
        MarkAsUpdated();
    }

    /// <summary>
    /// Принудительно завершить все сессии пользователя (инвалидировать выданные JWT).
    /// </summary>
    public void RotateSecurityStamp()
    {
        SecurityStamp = NewStamp();
    }

    public bool IsAdmin => Role == Role.Admin;
    public bool IsUser => Role == Role.User;

    private static string NewStamp() => Guid.NewGuid().ToString("N");

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}