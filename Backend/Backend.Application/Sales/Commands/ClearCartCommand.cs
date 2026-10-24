using MediatR;

namespace Backend.Application.Sales.Commands;

public class ClearCartCommand : IRequest
{
    public int UserId { get; set; }

    public ClearCartCommand(int userId)
    {
        UserId = userId;
    }
}