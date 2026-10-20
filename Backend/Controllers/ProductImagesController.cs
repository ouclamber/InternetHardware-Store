using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("product/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
    {
        var images = await _mediator.Send(new GetProductImagesQuery(productId), ct);
        return Ok(images);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add(
        [FromBody] AddProductImageRequest request,
        CancellationToken ct)
    {
        var imageId = await _mediator.Send(
            new AddProductImageCommand(
                request.ProductId,
                request.ImageUrl,
                request.AltText,
                request.IsMain),
            ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Изображение добавлено",
            imageId
        });
    }

    [HttpPut("{imageId:int}/main")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetMain(
        int imageId,
        [FromQuery] int productId,
        CancellationToken ct)
    {
        await _mediator.Send(new SetMainImageCommand(productId, imageId), ct);

        return Ok(new { message = "Изображение назначено главным" });
    }

    [HttpDelete("{imageId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int imageId,
        [FromQuery] int productId,
        CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductImageCommand(productId, imageId), ct);

        return Ok(new { message = "Изображение удалено" });
    }
}

public class AddProductImageRequest
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
}