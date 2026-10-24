using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

using Backend.Application.Catalog.Commands;
using Backend.Application.Catalog.Queries;
using Backend.Caching;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Controllers;

public class ProductsController : ApiControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger) : base(mediator)
    {
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int limit = 0,
        CancellationToken ct = default)
    {
        var products = await Mediator.Send(new GetProductsQuery { Limit = limit }, ct);
        return Ok(products);
    }

    /// <summary>
    /// GET api/products/paged — каталог с фильтрами, сортировкой и пагинацией на сервере.
    /// Пример: ?categoryId=3&amp;brandIds=1&amp;brandIds=2&amp;minPrice=10000&amp;inStockOnly=true&amp;sort=price_asc&amp;page=2&amp;pageSize=12
    /// sort: price_asc | price_desc | name_asc | name_desc | newest
    /// </summary>
    [HttpGet("paged")]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int? categoryId,
        [FromQuery] int? typeId,
        [FromQuery] int[]? brandIds,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? search,
        [FromQuery] bool inStockOnly = false,
        [FromQuery] string? sort = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetProductsPagedQuery
        {
            CategoryId = categoryId,
            TypeId = typeId,
            BrandIds = brandIds,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Search = search,
            InStockOnly = inStockOnly,
            Sort = sort,
            Page = page,
            PageSize = pageSize
        }, ct);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await Mediator.Send(new GetProductByIdQuery(id), ct);

        if (product == null)
            return NotFound(new { message = $"Товар с Id={id} не найден" });

        // Снятый с продажи товар видит только администратор
        if (!product.IsActive && !IsAdmin)
            return NotFound(new { message = $"Товар с Id={id} не найден" });

        // Ответ для администратора о снятом товаре не должен попасть в общий кеш
        if (!product.IsActive)
            CatalogCache.SkipStorage(HttpContext);

        return Ok(product);
    }

    [HttpGet("category/{categoryId:int}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> GetByCategory(int categoryId, CancellationToken ct)
    {
        var products = await Mediator.Send(new GetProductsByCategoryQuery(categoryId), ct);
        return Ok(products);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [OutputCache(PolicyName = CatalogCache.PolicyName)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Поисковый запрос не может быть пустым" });

        if (q.Length > ProductQuery.MaxSearchLength)
            return BadRequest(new { message = $"Поисковый запрос не длиннее {ProductQuery.MaxSearchLength} символов" });

        var products = await Mediator.Send(new SearchProductsQuery(q), ct);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
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
            TypeId = request.TypeId,
            StockQuantity = request.StockQuantity
        };

        var productId = await Mediator.Send(command, ct);

        return Ok(new
        {
            isSuccess = true,
            message = "Товар создан",
            productId
        });
    }

    /// <summary>PUT api/products/{id} — полное редактирование карточки товара.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new UpdateProductCommand
        {
            ProductId = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            TypeId = request.TypeId
        }, ct);

        return Ok(new { message = "Товар обновлён" });
    }

    [HttpPut("{id:int}/price")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> UpdatePrice(
        int id,
        [FromBody] UpdatePriceRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new UpdateProductPriceCommand(id, request.NewPrice), ct);

        return Ok(new { message = "Цена обновлена" });
    }

    /// <summary>PUT api/products/{id}/stock — установить остаток на складе.</summary>
    [HttpPut("{id:int}/stock")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> UpdateStock(
        int id,
        [FromBody] UpdateStockRequest request,
        CancellationToken ct)
    {
        await Mediator.Send(new SetProductStockCommand(id, request.Quantity), ct);

        return Ok(new { message = "Остаток обновлён" });
    }

    /// <summary>POST api/products/{id}/activate — вернуть товар в продажу.</summary>
    [HttpPost("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Mediator.Send(new ActivateProductCommand(id), ct);

        return Ok(new { message = "Товар снова в продаже" });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [EvictCatalogCache]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Mediator.Send(new DeactivateProductCommand(id), ct);

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
    public int StockQuantity { get; set; }
}

public class UpdateProductRequest
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

public class UpdateStockRequest
{
    public int Quantity { get; set; }
}
