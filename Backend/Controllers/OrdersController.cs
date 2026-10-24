using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Queries;
using Backend.Caching;

namespace Backend.Controllers;

[Authorize]
public class OrdersController : ApiControllerBase
{
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        var orders = await Mediator.Send(new GetUserOrdersQuery(userId), ct);
        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        var order = await Mediator.Send(new GetOrderByIdQuery(id, userId, IsAdmin), ct);

        if (order == null)
            return NotFound(new { message = $"Заказ с Id={id} не найден" });

        return Ok(order);
    }

    /// <summary>Оформить заказ из корзины. Остатки товаров списываются со склада.</summary>
    [HttpPost]
    [EvictCatalogCache]   // изменились остатки — списки каталога должны это показать
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

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

        var orderId = await Mediator.Send(command, ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Заказ оформлен",
            orderId
        });
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]   // при отмене товар возвращается на склад
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(
            new UpdateOrderStatusCommand(id, request.NewStatus, CurrentUserId),
            ct);

        return Ok(new { message = "Статус обновлён" });
    }

    [HttpDelete("{id:int}")]
    [EvictCatalogCache]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new CancelOrderCommand(id, userId), ct);

        return Ok(new { message = "Заказ отменён" });
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
