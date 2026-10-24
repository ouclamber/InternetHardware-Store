using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class ProductType : Entity
{
    public string Name { get; private set; } = string.Empty;

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Для EF Core
    protected ProductType() { }

    public ProductType(string name)
    {
        SetName(name);
    }

    public void Rename(string newName)
    {
        SetName(newName);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название типа обязательно");

        var trimmed = name.Trim();

        if (trimmed.Length > 100)
            throw new DomainException("Название типа не может быть длиннее 100 символов");

        Name = trimmed;
    }
}