using MediatR;

namespace Backend.Application.Catalog.Commands;

public class CreateCategoryCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }

    public CreateCategoryCommand(
        string name,
        string? description = null,
        string? imageUrl = null,
        int? parentCategoryId = null)
    {
        Name = name;
        Description = description;
        ImageUrl = imageUrl;
        ParentCategoryId = parentCategoryId;
    }
}