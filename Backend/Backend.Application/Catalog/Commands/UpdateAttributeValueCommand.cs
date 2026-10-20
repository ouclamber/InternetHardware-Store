using MediatR;

namespace Backend.Application.Catalog.Commands;

public class UpdateAttributeValueCommand : IRequest
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string NewValue { get; set; } = string.Empty;

    public UpdateAttributeValueCommand(int productId, int attributeId, string newValue)
    {
        ProductId = productId;
        AttributeId = attributeId;
        NewValue = newValue;
    }
}