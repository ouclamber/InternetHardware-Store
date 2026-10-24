using MediatR;

namespace Backend.Application.Admin.Commands;

public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }

    /// <summary>Id администратора, выполняющего операцию (удалить себя нельзя).</summary>
    public int RequestingUserId { get; set; }

    public DeleteUserCommand(int userId, int requestingUserId = 0)
    {
        UserId = userId;
        RequestingUserId = requestingUserId;
    }
}
