using MediatR;

namespace Backend.Application.Catalog.Commands;

/// <summary>Установить остаток товара на складе (админ).</summary>
public class SetProductStockCommand : IRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public SetProductStockCommand(int productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
