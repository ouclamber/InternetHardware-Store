using MediatR;

namespace Backend.Application.Admin.Commands;

public class UpdateUserRoleCommand : IRequest
{
    public int UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;

    /// <summary>Id администратора, выполняющего операцию (себе роль менять нельзя).</summary>
    public int RequestingUserId { get; set; }

    public UpdateUserRoleCommand(int userId, string newRole, int requestingUserId = 0)
    {
        UserId = userId;
        NewRole = newRole;
        RequestingUserId = requestingUserId;
    }
}
