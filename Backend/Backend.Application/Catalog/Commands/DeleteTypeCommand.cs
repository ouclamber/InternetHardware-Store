using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeleteTypeCommand : IRequest
{
    public int TypeId { get; set; }

    public DeleteTypeCommand(int typeId)
    {
        TypeId = typeId;
    }
}