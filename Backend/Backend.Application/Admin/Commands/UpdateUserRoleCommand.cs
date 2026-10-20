using MediatR;

namespace Backend.Application.Admin.Commands;

public class UpdateUserRoleCommand : IRequest
{
    public int UserId { get; set; }
    public string NewRole { get; set; } = string.Empty;

    public UpdateUserRoleCommand(int userId, string newRole)
    {
        UserId = userId;
        NewRole = newRole;
    }
}