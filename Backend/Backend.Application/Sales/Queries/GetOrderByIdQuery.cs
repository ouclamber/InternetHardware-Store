using MediatR;
using Backend.Application.Sales.DTOs;

namespace Backend.Application.Sales.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto?>
{
    public int OrderId { get; set; }
    public int RequestingUserId { get; set; }
    public bool IsAdmin { get; set; }

    public GetOrderByIdQuery(int orderId, int requestingUserId, bool isAdmin = false)
    {
        OrderId = orderId;
        RequestingUserId = requestingUserId;
        IsAdmin = isAdmin;
    }
}