using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class ProductAttributeValue : Entity
{
    public int ProductId { get; private set; }
    public int AttributeId { get; private set; }
    public string Value { get; private set; } = string.Empty;

    public Product? Product { get; private set; }
    public ProductAttribute? ProductAttributes { get; private set; }

    // Для EF Core
    protected ProductAttributeValue() { }

    internal ProductAttributeValue(int productId, int attributeId, string value)
    {
        ProductId = productId;
        AttributeId = attributeId;
        SetValue(value);
    }

    public void UpdateValue(string newValue)
    {
        SetValue(newValue);
    }

    private void SetValue(string value)
    {
        if (value == null)
            throw new DomainException("Значение атрибута не может быть null");

        var trimmed = value.Trim();

        if (trimmed.Length > 500)
            throw new DomainException("Значение атрибута не может быть длиннее 500 символов");

        Value = trimmed;
    }
}