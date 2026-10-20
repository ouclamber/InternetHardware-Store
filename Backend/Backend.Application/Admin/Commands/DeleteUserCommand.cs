using MediatR;

namespace Backend.Application.Admin.Commands;

public class DeleteUserCommand : IRequest
{
    public int UserId { get; set; }

    public DeleteUserCommand(int userId)
    {
        UserId = userId;
    }
}