using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        var orders = await _mediator.Send(new GetUserOrdersQuery(userId), ct);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        var order = await _mediator.Send(new GetOrderByIdQuery(id, userId, isAdmin), ct);

        if (order == null)
            return NotFound(new { message = $"Заказ с Id={id} не найден" });

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        _logger.LogInformation("Создание заказа для UserId={UserId}", userId);

        var command = new CreateOrderCommand
        {
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            DeliveryMethod = request.DeliveryMethod,
            PaymentMethod = request.PaymentMethod,
            Comment = request.Comment
        };

        var orderId = await _mediator.Send(command, ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Заказ оформлен",
            orderId
        });
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        var adminUserId = GetCurrentUserId();

        await _mediator.Send(
            new UpdateOrderStatusCommand(id, request.NewStatus, adminUserId),
            ct);

        return Ok(new { message = "Статус обновлён" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(new CancelOrderCommand(id, userId), ct);

        return Ok(new { message = "Заказ отменён" });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : -1;
    }
}

public class CreateOrderRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string DeliveryMethod { get; set; } = "courier";
    public string PaymentMethod { get; set; } = "card";
    public string? Comment { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
}