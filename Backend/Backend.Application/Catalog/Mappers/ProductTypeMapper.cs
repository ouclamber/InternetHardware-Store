using Backend.Application.Catalog.DTOs;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Catalog.Mappers;

internal static class ProductTypeMapper
{
    public static ProductTypeDto MapToDto(ProductType type)
    {
        return new ProductTypeDto
        {
            Id = type.Id,
            Name = type.Name
        };
    }
}