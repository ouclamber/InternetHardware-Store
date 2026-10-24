using MediatR;

namespace Backend.Application.Sales.Commands;

public class UpdateOrderStatusCommand : IRequest
{
    public int OrderId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public int AdminUserId { get; set; }

    public UpdateOrderStatusCommand(int orderId, string newStatus, int adminUserId)
    {
        OrderId = orderId;
        NewStatus = newStatus;
        AdminUserId = adminUserId;
    }
}