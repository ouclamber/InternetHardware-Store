using MediatR;

namespace Backend.Application.Catalog.Commands;

/// <summary>Вернуть деактивированный товар в продажу (админ).</summary>
public class ActivateProductCommand : IRequest
{
    public int ProductId { get; set; }

    public ActivateProductCommand(int productId)
    {
        ProductId = productId;
    }
}
