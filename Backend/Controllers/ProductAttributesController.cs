using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Backend.Caching;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

public class ProductAttributesController : ApiControllerBase
{
    public ProductAttributesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var attributes = await Mediator.Send(new GetProductAttributesQuery(), ct);
        return Ok(attributes);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var attribute = await Mediator.Send(new GetProductAttributeByIdQuery(id), ct);

        if (attribute == null)
            return NotFound(new { message = $"Атрибут с Id={id} не найден" });

        return Ok(attribute);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductAttributeRequest request,
        CancellationToken ct)
    {
        var id = await Mediator.Send(
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
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetByProduct(int productId, CancellationToken ct)
    {
        var values = await Mediator.Send(new GetProductValuesQuery(productId), ct);
        return Ok(values);
    }

    [HttpPost("value")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> AddValue(
        [FromBody] AddAttributeValueRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(
            new AddAttributeValueCommand(request.ProductId, request.AttributeId, request.Value),
            ct);

        return Ok(new { message = "Значение добавлено" });
    }

    [HttpPut("value")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> UpdateValue(
        [FromBody] UpdateAttributeValueRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(
            new UpdateAttributeValueCommand(request.ProductId, request.AttributeId, request.NewValue),
            ct);

        return Ok(new { message = "Значение обновлено" });
    }

    [HttpDelete("value")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> DeleteValue(
        [FromQuery] int productId,
        [FromQuery] int attributeId,
        CancellationToken ct)
    {
        await Mediator.Send(new DeleteAttributeValueCommand(productId, attributeId), ct);

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