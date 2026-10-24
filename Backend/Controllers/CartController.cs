using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Queries;

namespace Backend.Controllers;

/// <summary>
/// Корзина текущего пользователя. Все endpoint-ы требуют авторизации — UserId берётся из JWT.
/// </summary>
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ILogger<CartController> _logger;

    public CartController(IMediator mediator, ILogger<CartController> logger) : base(mediator)
    {
        _logger = logger;
    }

    /// <summary>GET api/cart — корзина текущего пользователя.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        var cart = await Mediator.Send(new GetCartQuery(userId), ct);
        return Ok(cart);
    }

    /// <summary>POST api/cart/items — добавить товар. Body: { productId: 1, quantity: 1 }</summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartRequest request,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        _logger.LogInformation("Добавление товара {ProductId} в корзину пользователя {UserId}",
            request.ProductId, userId);

        var itemId = await Mediator.Send(
            new AddToCartCommand(userId, request.ProductId, request.Quantity),
            ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Товар добавлен в корзину",
            itemId
        });
    }

    /// <summary>
    /// PUT api/cart/items/{productId} — изменить количество. Body: { quantity: 3 }.
    /// Если quantity ≤ 0 — товар удаляется.
    /// </summary>
    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateQuantity(
        int productId,
        [FromBody] UpdateCartQuantityRequest request,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(
            new UpdateCartQuantityCommand(userId, productId, request.Quantity),
            ct);

        return Ok(new { message = "Количество обновлено" });
    }

    /// <summary>DELETE api/cart/items/{productId} — удалить товар из корзины.</summary>
    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new RemoveFromCartCommand(userId, productId), ct);

        return Ok(new { message = "Товар удалён из корзины" });
    }

    /// <summary>DELETE api/cart — очистить всю корзину.</summary>
    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new ClearCartCommand(userId), ct);

        return Ok(new { message = "Корзина очищена" });
    }
}

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartQuantityRequest
{
    public int Quantity { get; set; }
}
