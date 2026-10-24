using Backend.Application.Catalog.DTOs;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Catalog.Mappers;

internal static class ProductAttributeMapper
{
    public static ProductAttributeDto MapToDto(ProductAttribute attribute)
    {
        return new ProductAttributeDto
        {
            Id = attribute.Id,
            Name = attribute.Name,
            Group = attribute.AttributeGroup,
            Unit = attribute.Unit
        };
    }
}