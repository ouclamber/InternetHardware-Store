using Backend.Application.Catalog.DTOs;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Catalog.Mappers;

internal static class ProductImageMapper
{
    public static ProductImageDto MapToDto(ProductImage image)
    {
        return new ProductImageDto
        {
            Id = image.Id,
            ProductId = image.ProductId,
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            IsMain = image.IsMain
        };
    }
}