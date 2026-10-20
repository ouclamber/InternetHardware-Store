using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var brands = await _mediator.Send(new GetBrandsQuery(), ct);
        return Ok(brands);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var brand = await _mediator.Send(new GetBrandByIdQuery(id), ct);

        if (brand == null)
            return NotFound(new { message = $"Бренд с Id={id} не найден" });

        return Ok(brand);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request, CancellationToken ct)
    {
        var brandId = await _mediator.Send(new CreateBrandCommand(request.Name), ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Бренд создан",
            brandId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateBrandCommand(id, request.Name), ct);

        return Ok(new { message = "Бренд обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBrandCommand(id), ct);

        return Ok(new { message = "Бренд удалён" });
    }
}


public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateBrandRequest
{
    public string Name { get; set; } = string.Empty;
}