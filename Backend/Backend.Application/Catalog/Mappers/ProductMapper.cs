using Backend.Application.DTOs;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Catalog.Mappers;

internal static class ProductMapper
{
    public static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description,
            Price = product.Price.Amount,
            IsActive = product.IsActive,

            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,

            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,

            TypeId = product.TypeId,
            TypeName = product.Type?.Name,

            Images = product.Images
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsMain = i.IsMain
                })
                .ToList(),

            Attributes = product.Values
                .Select(v => new ProductAttributeDto
                {
                    AttributeId = v.AttributeId,
                    AttributeName = v.ProductAttributes?.Name ?? string.Empty,
                    Group = v.ProductAttributes?.AttributeGroup,
                    Unit = v.ProductAttributes?.Unit,
                    Value = v.Value
                })
                .ToList()
        };
    }
}