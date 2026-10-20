using MediatR;

namespace Backend.Application.Catalog.Commands;

public class UpdateProductPriceCommand : IRequest
{
    public int ProductId { get; set; }
    public decimal NewPrice { get; set; }

    public UpdateProductPriceCommand(int productId, decimal newPrice)
    {
        ProductId = productId;
        NewPrice = newPrice;
    }
}