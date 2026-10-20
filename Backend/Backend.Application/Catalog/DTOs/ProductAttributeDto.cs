namespace Backend.Application.Catalog.DTOs;

public class ProductAttributeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Group { get; set; }
    public string? Unit { get; set; }
}