using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeleteCategoryCommand : IRequest
{
    public int CategoryId { get; set; }

    public DeleteCategoryCommand(int categoryId)
    {
        CategoryId = categoryId;
    }
}