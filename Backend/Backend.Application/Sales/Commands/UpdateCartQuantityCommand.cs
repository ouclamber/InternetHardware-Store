using MediatR;

namespace Backend.Application.Sales.Commands;

public class UpdateCartQuantityCommand : IRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int NewQuantity { get; set; }

    public UpdateCartQuantityCommand(int userId, int productId, int newQuantity)
    {
        UserId = userId;
        ProductId = productId;
        NewQuantity = newQuantity;
    }
}