using MediatR;
using Backend.Application.Sales.DTOs;

namespace Backend.Application.Sales.Queries;

public class GetUserOrdersQuery : IRequest<IReadOnlyList<OrderDto>>
{
    public int UserId { get; set; }

    public GetUserOrdersQuery(int userId)
    {
        UserId = userId;
    }
}