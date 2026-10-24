namespace Backend.Domain.Users;

public enum Role
{
    User = 1,
    Admin = 2
}

public static class RoleExtensions
{
    public static string ToCode(this Role role) => role switch
    {
        Role.User => "User",
        Role.Admin => "Admin",
        _ => "User"
    };

    public static Role FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Role.User;

        return code.Trim() switch
        {
            "Admin" => Role.Admin,
            "User" => Role.User,
            _ => throw new ArgumentException($"Неизвестная роль: {code}", nameof(code))
        };
    }

    public static bool IsValid(string code)
    {
        return code == "Admin" || code == "User";
    }
}