using MediatR;

namespace Backend.Application.Sales.Commands;

public class AddToCartCommand : IRequest<int>
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;

    public AddToCartCommand(int userId, int productId, int quantity = 1)
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
    }
}