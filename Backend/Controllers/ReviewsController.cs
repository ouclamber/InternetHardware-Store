using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Reviews.Commands;
using Backend.Application.Reviews.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("product/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductReviewsQuery(productId), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        _logger.LogInformation("Создание отзыва: UserId={UserId}, ProductId={ProductId}",
            userId, request.ProductId);

        var reviewId = await _mediator.Send(
            new CreateReviewCommand(userId, request.ProductId, request.Rating, request.Comment),
            ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Отзыв успешно добавлен",
            reviewId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReviewRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(
            new UpdateReviewCommand(id, userId, isAdmin, request.Rating, request.Comment),
            ct);

        return Ok(new { message = "Отзыв обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (userId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(new DeleteReviewCommand(id, userId, isAdmin), ct);

        return Ok(new { message = "Отзыв удалён" });
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var adminUserId = GetCurrentUserId();
        if (adminUserId <= 0)
            return Unauthorized(new { message = "Пользователь не авторизован" });

        await _mediator.Send(new ApproveReviewCommand(id, adminUserId), ct);

        return Ok(new { message = "Отзыв одобрен" });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : -1;
    }
}

public class CreateReviewRequest
{
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}