using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Backend.Application.Users.Commands;
using Backend.Application.Users.DTOs;

namespace Backend.Controllers;

[EnableRateLimiting("auth")]
public class AuthController : ApiControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(
        [FromBody] SignUpCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SignUp: {UserName}", command.UserName);

        var userId = await Mediator.Send(command, cancellationToken);

        return Ok(new
        {
            isSuccess = true,
            message = "Регистрация прошла успешно",
            userId
        });
    }

    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SignIn: {UserName}", command.UserName);

        AuthResultDto result = await Mediator.Send(command, cancellationToken);

        return Ok(ToResponse(result));
    }

    /// <summary>
    /// Смена пароля. Все остальные сессии пользователя завершаются,
    /// в ответе — новый токен для текущей.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        _logger.LogInformation("ChangePassword для UserId: {UserId}", userId);

        var result = await Mediator.Send(
            new ChangePasswordCommand(userId, request.OldPassword, request.NewPassword),
            cancellationToken);

        return Ok(ToResponse(result));
    }

    private static object ToResponse(AuthResultDto result) => new
    {
        isSuccess = result.IsSuccess,
        message = result.Message,
        token = result.Token,
        userId = result.UserId,
        userName = result.UserName,
        role = result.Role
    };
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
