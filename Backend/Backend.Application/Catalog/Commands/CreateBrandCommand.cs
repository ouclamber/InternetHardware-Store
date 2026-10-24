using MediatR;

namespace Backend.Application.Catalog.Commands;

public class CreateBrandCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;

    public CreateBrandCommand(string name)
    {
        Name = name;
    }
}