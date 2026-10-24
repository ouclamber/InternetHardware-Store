using MediatR;

namespace Backend.Application.Catalog.Commands;

public class CreateProductAttributeCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Unit { get; set; }

    public CreateProductAttributeCommand(string name, string? group = null, string? unit = null)
    {
        Name = name;
        Group = group;
        Unit = unit;
    }
}