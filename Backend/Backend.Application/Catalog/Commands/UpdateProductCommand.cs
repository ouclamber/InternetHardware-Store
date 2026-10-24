using MediatR;

namespace Backend.Application.Catalog.Commands;

/// <summary>
/// Полное редактирование карточки товара (админ).
/// </summary>
public class UpdateProductCommand : IRequest
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int BrandId { get; set; }
    public int CategoryId { get; set; }
    public int TypeId { get; set; }
}
