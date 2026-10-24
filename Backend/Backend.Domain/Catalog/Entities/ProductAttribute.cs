using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class ProductAttribute : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? AttributeGroup { get; private set; }
    public string? Unit { get; private set; }

    private readonly List<ProductAttributeValue> _values = new();
    public IReadOnlyCollection<ProductAttributeValue> Values => _values.AsReadOnly();

    // Для EF Core
    protected ProductAttribute() { }

    public ProductAttribute(string name, string? attributeGroup = null, string? unit = null)
    {
        SetName(name);
        AttributeGroup = attributeGroup?.Trim();
        Unit = unit?.Trim();
    }

    public void Update(string name, string? attributeGroup, string? unit)
    {
        SetName(name);
        AttributeGroup = attributeGroup?.Trim();
        Unit = unit?.Trim();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название атрибута обязательно");

        var trimmed = name.Trim();

        if (trimmed.Length > 100)
            throw new DomainException("Название атрибута не может быть длиннее 100 символов");

        Name = trimmed;
    }
}