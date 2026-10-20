using MediatR;

namespace Backend.Application.Sales.Commands;

public class CancelOrderCommand : IRequest
{
    public int OrderId { get; set; }
    public int UserId { get; set; }

    public CancelOrderCommand(int orderId, int userId)
    {
        OrderId = orderId;
        UserId = userId;
    }
}