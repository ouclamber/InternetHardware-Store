using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Queries;

namespace Backend.Controllers;

/// <summary>
/// Контроллер корзины.
/// 
/// Все endpoint-ы требуют авторизации — UserId берётся из JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CartController> _logger;

    public CartController(IMediator mediator, ILogger<CartController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// GET api/cart — корзина текущего пользователя.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyCart(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        var cart = await _mediator.Send(new GetCartQuery(userId), ct);
        return Ok(cart);
    }

    /// <summary>
    /// POST api/cart/items — добавить товар в корзину.
    /// Body: { productId: 1, quantity: 1 }
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        _logger.LogInformation("Добавление товара {ProductId} в корзину пользователя {UserId}",
            request.ProductId, userId);

        var itemId = await _mediator.Send(
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
    /// PUT api/cart/items/{productId} — изменить количество товара.
    /// Body: { quantity: 3 }
    /// Если quantity ≤ 0 — товар удаляется.
    /// </summary>
    [HttpPut("items/{productId:int}")]
    public async Task<IActionResult> UpdateQuantity(
        int productId,
        [FromBody] UpdateCartQuantityRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(
            new UpdateCartQuantityCommand(userId, productId, request.Quantity),
            ct);

        return Ok(new { message = "Количество обновлено" });
    }

    /// <summary>
    /// DELETE api/cart/items/{productId} — удалить товар из корзины.
    /// </summary>
    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int productId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(new RemoveFromCartCommand(userId, productId), ct);

        return Ok(new { message = "Товар удалён из корзины" });
    }

    /// <summary>
    /// DELETE api/cart — очистить всю корзину.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(new ClearCartCommand(userId), ct);

        return Ok(new { message = "Корзина очищена" });
    }

    // === Helpers ===

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : -1;
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