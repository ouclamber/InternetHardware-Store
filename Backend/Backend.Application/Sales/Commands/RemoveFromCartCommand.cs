using MediatR;

namespace Backend.Application.Sales.Commands;

public class RemoveFromCartCommand : IRequest
{
    public int UserId { get; set; }
    public int ProductId { get; set; }

    public RemoveFromCartCommand(int userId, int productId)
    {
        UserId = userId;
        ProductId = productId;
    }
}