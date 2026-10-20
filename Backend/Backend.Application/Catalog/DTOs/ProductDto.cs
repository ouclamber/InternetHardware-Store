namespace Backend.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }

    // Brand
    public int? BrandId { get; set; }
    public string? BrandName { get; set; }

    // Category
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Type
    public int? TypeId { get; set; }
    public string? TypeName { get; set; }

    // Images
    public List<ProductImageDto> Images { get; set; } = new();

    // Attributes
    public List<ProductAttributeDto> Attributes { get; set; } = new();
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
}

public class ProductAttributeDto
{
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Unit { get; set; }
    public string Value { get; set; } = string.Empty;
}