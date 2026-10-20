using MediatR;

namespace Backend.Application.Users.Commands;

public class ChangePasswordCommand : IRequest
{
    public int UserId { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public ChangePasswordCommand(int userId, string oldPassword, string newPassword)
    {
        UserId = userId;
        OldPassword = oldPassword;
        NewPassword = newPassword;
    }
}