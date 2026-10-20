using MediatR;

namespace Backend.Application.Catalog.Commands;

public class CreateTypeCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;

    public CreateTypeCommand(string name)
    {
        Name = name;
    }
}