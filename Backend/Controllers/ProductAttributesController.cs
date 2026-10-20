using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductAttributesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductAttributesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var attributes = await _mediator.Send(new GetProductAttributesQuery(), ct);
        return Ok(attributes);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var attribute = await _mediator.Send(new GetProductAttributeByIdQuery(id), ct);

        if (attribute == null)
            return NotFound(new { message = $"Атрибут с Id={id} не найден" });

        return Ok(attribute);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductAttributeRequest request,
        CancellationToken ct)
    {
        var id = await _mediator.Send(
            new CreateProductAttributeCommand(request.Name, request.Group, request.Unit),
            ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Атрибут создан",
            attributeId = id
        });
    }

    [HttpGet("product/{productId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
    {
        var values = await _mediator.Send(new GetProductValuesQuery(productId), ct);
        return Ok(values);
    }

    [HttpPost("value")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddValue(
        [FromBody] AddAttributeValueRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(
            new AddAttributeValueCommand(request.ProductId, request.AttributeId, request.Value),
            ct);

        return Ok(new { message = "Значение добавлено" });
    }

    [HttpPut("value")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateValue(
        [FromBody] UpdateAttributeValueRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateAttributeValueCommand(request.ProductId, request.AttributeId, request.NewValue),
            ct);

        return Ok(new { message = "Значение обновлено" });
    }

    [HttpDelete("value")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteValue(
        [FromQuery] int productId,
        [FromQuery] int attributeId,
        CancellationToken ct)
    {
        await _mediator.Send(new DeleteAttributeValueCommand(productId, attributeId), ct);

        return Ok(new { message = "Значение удалено" });
    }
}

public class CreateProductAttributeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Unit { get; set; }
}

public class AddAttributeValueRequest
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class UpdateAttributeValueRequest
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string NewValue { get; set; } = string.Empty;
}