using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Backend.Caching;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

public class BrandsController : ApiControllerBase
{
    public BrandsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var brands = await Mediator.Send(new GetBrandsQuery(), ct);
        return Ok(brands);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var brand = await Mediator.Send(new GetBrandByIdQuery(id), ct);

        if (brand == null)
            return NotFound(new { message = $"Бренд с Id={id} не найден" });

        return Ok(brand);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request, CancellationToken ct)
    {
        var brandId = await Mediator.Send(new CreateBrandCommand(request.Name), ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Бренд создан",
            brandId
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBrandRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateBrandCommand(id, request.Name), ct);

        return Ok(new { message = "Бренд обновлён" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteBrandCommand(id), ct);

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