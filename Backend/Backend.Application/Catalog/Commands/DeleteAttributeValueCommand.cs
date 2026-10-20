using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeleteAttributeValueCommand : IRequest
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }

    public DeleteAttributeValueCommand(int productId, int attributeId)
    {
        ProductId = productId;
        AttributeId = attributeId;
    }
}