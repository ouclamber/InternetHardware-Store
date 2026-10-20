using MediatR;
using Backend.Application.Sales.DTOs;

namespace Backend.Application.Sales.Queries;

public class GetCartQuery : IRequest<CartDto?>
{
    public int UserId { get; set; }

    public GetCartQuery(int userId)
    {
        UserId = userId;
    }
}