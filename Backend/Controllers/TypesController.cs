using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var types = await _mediator.Send(new GetTypesQuery(), ct);
        return Ok(types);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var type = await _mediator.Send(new GetTypeByIdQuery(id), ct);

        if (type == null)
            return NotFound(new { message = $"Тип с Id={id} не найден" });

        return Ok(type);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTypeRequest request, CancellationToken ct)
    {
        var typeId = await _mediator.Send(new CreateTypeCommand(request.Name), ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Тип создан",
            typeId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTypeRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateTypeCommand(id, request.Name), ct);

        return Ok(new { message = "Тип обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTypeCommand(id), ct);

        return Ok(new { message = "Тип удалён" });
    }
}

public class CreateTypeRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateTypeRequest
{
    public string Name { get; set; } = string.Empty;
}