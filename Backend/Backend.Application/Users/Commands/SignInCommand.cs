using MediatR;
using Backend.Application.Users.DTOs;

namespace Backend.Application.Users.Commands;

public class SignInCommand : IRequest<AuthResultDto>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Необязательная ожидаемая роль (форма входа администратора передаёт "Admin").
    /// Роль в токене всегда берётся из БД, а не из запроса.
    /// </summary>
    public string? Role { get; set; }
}
