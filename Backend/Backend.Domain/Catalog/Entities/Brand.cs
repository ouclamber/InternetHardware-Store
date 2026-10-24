using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class Brand : Entity
{
    public string Name { get; private set; } = string.Empty;

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Для EF Core
    protected Brand() { }

    public Brand(string name)
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
            throw new DomainException("Название бренда обязательно");

        var trimmed = name.Trim();

        if (trimmed.Length > 100)
            throw new DomainException("Название бренда не может быть длиннее 100 символов");

        Name = trimmed;
    }
}