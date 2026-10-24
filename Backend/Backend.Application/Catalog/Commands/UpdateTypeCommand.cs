using MediatR;

namespace Backend.Application.Catalog.Commands;

public class UpdateTypeCommand : IRequest
{
    public int TypeId { get; set; }
    public string NewName { get; set; } = string.Empty;

    public UpdateTypeCommand(int typeId, string newName)
    {
        TypeId = typeId;
        NewName = newName;
    }
}