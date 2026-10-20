using MediatR;
using Backend.Application.Users.DTOs;

namespace Backend.Application.Users.Commands;

public class SignInCommand : IRequest<AuthResultDto>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}