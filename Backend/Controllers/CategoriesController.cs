using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool rootOnly = false,
        CancellationToken ct = default)
    {
        var categories = await _mediator.Send(new GetCategoriesQuery(rootOnly), ct);
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var category = await _mediator.Send(new GetCategoryByIdQuery(id), ct);

        if (category == null)
            return NotFound(new { message = $"Категория с Id={id} не найдена" });

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken ct)
    {
        var categoryId = await _mediator.Send(
            new CreateCategoryCommand(
                request.Name,
                request.Description,
                request.ImageUrl,
                request.ParentCategoryId),
            ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Категория создана",
            categoryId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateCategoryCommand(id, request.Name, request.Description, request.ImageUrl),
            ct);

        return Ok(new { message = "Категория обновлена" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), ct);

        return Ok(new { message = "Категория удалена" });
    }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}