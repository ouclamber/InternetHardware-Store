using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

// Application — команды и DTO
using Backend.Application.Users.Commands;
using Backend.Application.Users.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp(
        [FromBody] SignUpCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SignUp: {UserName}", command.UserName);

        var userId = await _mediator.Send(command, cancellationToken);

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
        _logger.LogInformation("SignIn: {UserName}, Role: {Role}",
            command.UserName, command.Role);

        AuthResultDto result = await _mediator.Send(command, cancellationToken);

        return Ok(new
        {
            isSuccess = result.IsSuccess,
            message = result.Message,
            token = result.Token,
            userId = result.UserId,
            userName = result.UserName,
            role = result.Role
        });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Пользователь не авторизован" });

        _logger.LogInformation("ChangePassword для UserId: {UserId}", userId);

        var command = new ChangePasswordCommand(
            userId,
            request.OldPassword,
            request.NewPassword);

        await _mediator.Send(command, cancellationToken);

        return Ok(new { message = "Пароль успешно изменён" });
    }
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}