using MediatR;

namespace Backend.Application.Catalog.Commands;

public class UpdateCategoryCommand : IRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    public UpdateCategoryCommand(
        int categoryId,
        string name,
        string? description = null,
        string? imageUrl = null)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
    }
}