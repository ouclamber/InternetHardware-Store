using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Backend.Caching;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

public class TypesController : ApiControllerBase
{
    public TypesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var types = await Mediator.Send(new GetTypesQuery(), ct);
        return Ok(types);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var type = await Mediator.Send(new GetTypeByIdQuery(id), ct);

        if (type == null)
            return NotFound(new { message = $"Тип с Id={id} не найден" });

        return Ok(type);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Create([FromBody] CreateTypeRequest request, CancellationToken ct)
    {
        var typeId = await Mediator.Send(new CreateTypeCommand(request.Name), ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Тип создан",
            typeId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTypeRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateTypeCommand(id, request.Name), ct);

        return Ok(new { message = "Тип обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTypeCommand(id), ct);

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