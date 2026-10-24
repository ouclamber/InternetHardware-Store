using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Общая база контроллеров: доступ к MediatR и текущему пользователю из JWT.
/// Раньше метод GetCurrentUserId был скопирован в каждый контроллер.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IMediator Mediator { get; }

    protected ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>Id пользователя из токена или -1, если не авторизован.</summary>
    protected int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst("userId")?.Value
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(claim, out var id) && id > 0 ? id : -1;
        }
    }

    protected bool IsAdmin => User.IsInRole("Admin");

    protected IActionResult UnauthorizedUser() =>
        Unauthorized(new { message = "Пользователь не авторизован" });
}
