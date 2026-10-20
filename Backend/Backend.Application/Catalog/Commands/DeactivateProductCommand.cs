using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeactivateProductCommand : IRequest
{
    public int ProductId { get; set; }

    public DeactivateProductCommand(int productId)
    {
        ProductId = productId;
    }
}