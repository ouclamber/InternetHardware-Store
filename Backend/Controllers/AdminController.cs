using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Admin.Commands;
using Backend.Application.Admin.Queries;

namespace Backend.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await Mediator.Send(new GetAdminStatsQuery(), ct);
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await Mediator.Send(new GetUsersQuery(), ct);
        return Ok(users);
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Админ {AdminId}: смена роли пользователя {UserId} на {Role}",
            CurrentUserId, id, request.NewRole);

        await Mediator.Send(new UpdateUserRoleCommand(id, request.NewRole, CurrentUserId), ct);

        return Ok(new { message = "Роль обновлена" });
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        _logger.LogInformation("Админ {AdminId}: удаление пользователя {UserId}", CurrentUserId, id);

        await Mediator.Send(new DeleteUserCommand(id, CurrentUserId), ct);

        return Ok(new { message = "Пользователь удалён" });
    }

    /// <summary>
    /// GET api/admin/orders?status=pending&amp;page=1&amp;pageSize=20 — заказы постранично.
    /// </summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var orders = await Mediator.Send(new GetAllOrdersQuery
        {
            Status = status,
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(orders);
    }
}

public class UpdateUserRoleRequest
{
    public string NewRole { get; set; } = string.Empty;
}
