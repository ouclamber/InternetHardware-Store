using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Reviews.Commands;
using Backend.Application.Reviews.Queries;

namespace Backend.Controllers;

public class ReviewsController : ApiControllerBase
{
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpGet("product/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetProductReviewsQuery(productId), ct);
        return Ok(result);
    }

    /// <summary>GET api/reviews/pending — отзывы на модерации (админ).</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPendingReviewsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        _logger.LogInformation("Создание отзыва: UserId={UserId}, ProductId={ProductId}",
            userId, request.ProductId);

        var reviewId = await Mediator.Send(
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
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(
            new UpdateReviewCommand(id, userId, IsAdmin, request.Rating, request.Comment),
            ct);

        return Ok(new { message = "Отзыв обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new DeleteReviewCommand(id, userId, IsAdmin), ct);

        return Ok(new { message = "Отзыв удалён" });
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var adminUserId = CurrentUserId;
        if (adminUserId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new ApproveReviewCommand(id, adminUserId), ct);

        return Ok(new { message = "Отзыв одобрен" });
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        var adminUserId = CurrentUserId;
        if (adminUserId <= 0)
            return UnauthorizedUser();

        await Mediator.Send(new RejectReviewCommand(id, adminUserId), ct);

        return Ok(new { message = "Отзыв отклонён" });
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
