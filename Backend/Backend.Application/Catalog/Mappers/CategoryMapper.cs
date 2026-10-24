using Backend.Application.Catalog.DTOs;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Catalog.Mappers;

internal static class CategoryMapper
{
    public static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name
        };
    }
}