using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 0,
        CancellationToken ct = default)
    {
        var products = await _mediator.Send(new GetProductsQuery { Limit = limit }, ct);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id), ct);

        if (product == null)
            return NotFound(new { message = $"Товар с Id={id} не найден" });

        return Ok(product);
    }

    [HttpGet("category/{categoryId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(int categoryId, CancellationToken ct)
    {
        var products = await _mediator.Send(new GetProductsByCategoryQuery(categoryId), ct);
        return Ok(products);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Поисковый запрос не может быть пустым" });

        var products = await _mediator.Send(new SearchProductsQuery(q), ct);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        _logger.LogInformation("Создание товара: {Name}, цена: {Price}",
            request.Name, request.Price);

        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            TypeId = request.TypeId
        };

        var productId = await _mediator.Send(command, ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Товар создан",
            productId
        });
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePrice(
        int id,
        [FromBody] UpdatePriceRequest request,
        CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice), ct);

        return Ok(new { message = "Цена обновлена" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateProductCommand(id), ct);

        return Ok(new { message = "Товар деактивирован" });
    }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public int TypeId { get; set; }
}

public class UpdatePriceRequest
{
    public decimal NewPrice { get; set; }
}