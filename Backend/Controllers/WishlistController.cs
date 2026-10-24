using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Wishlist.Commands;
using Backend.Application.Wishlist.Queries;

namespace Backend.Controllers;

/// <summary>
/// Избранное текущего пользователя.
/// </summary>
[Authorize]
public class WishlistController : ApiControllerBase
{
    public WishlistController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>GET api/wishlist — товары в избранном (новые сверху).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        return Ok(await Mediator.Send(new GetWishlistQuery(userId), ct));
    }

    /// <summary>GET api/wishlist/ids — только Id товаров (для «сердечек» в каталоге).</summary>
    [HttpGet("ids")]
    public async Task<IActionResult> GetIds(CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        return Ok(await Mediator.Send(new GetWishlistProductIdsQuery(userId), ct));
    }

    /// <summary>PUT api/wishlist/{productId} — добавить (идемпотентно).</summary>
    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Add(int productId, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new AddToWishlistCommand(userId, productId), ct);
        return Ok(new { message = "Товар добавлен в избранное" });
    }

    /// <summary>DELETE api/wishlist/{productId} — убрать (идемпотентно).</summary>
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new RemoveFromWishlistCommand(userId, productId), ct);
        return Ok(new { message = "Товар удалён из избранного" });
    }
}
