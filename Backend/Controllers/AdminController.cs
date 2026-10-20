using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Admin.Commands;
using Backend.Application.Admin.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _mediator.Send(new GetUsersQuery(), ct);
        return Ok(users);
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Смена роли пользователя {UserId} на {Role}", id, request.NewRole);

        await _mediator.Send(new UpdateUserRoleCommand(id, request.NewRole), ct);

        return Ok(new { message = "Роль обновлена" });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand(id), ct);

        return Ok(new { message = "Пользователь удалён" });
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(CancellationToken ct)
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery(), ct);
        return Ok(orders);
    }
}

public class UpdateUserRoleRequest
{
    public string NewRole { get; set; } = string.Empty;
}