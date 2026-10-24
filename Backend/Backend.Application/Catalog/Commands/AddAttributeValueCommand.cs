using MediatR;

namespace Backend.Application.Catalog.Commands;

public class AddAttributeValueCommand : IRequest
{
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string Value { get; set; } = string.Empty;

    public AddAttributeValueCommand(int productId, int attributeId, string value)
    {
        ProductId = productId;
        AttributeId = attributeId;
        Value = value;
    }
}