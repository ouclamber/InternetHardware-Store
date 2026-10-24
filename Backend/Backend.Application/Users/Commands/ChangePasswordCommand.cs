using MediatR;
using Backend.Application.Users.DTOs;

namespace Backend.Application.Users.Commands;

/// <summary>
/// Смена пароля. Все ранее выданные токены пользователя после этого недействительны,
/// поэтому в ответ возвращается новый токен для текущей сессии.
/// </summary>
public class ChangePasswordCommand : IRequest<AuthResultDto>
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
