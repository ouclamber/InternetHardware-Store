namespace Backend.Application.Catalog.DTOs;

public class ProductAttributeValueDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int AttributeId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Unit { get; set; }
    public string Value { get; set; } = string.Empty;
}