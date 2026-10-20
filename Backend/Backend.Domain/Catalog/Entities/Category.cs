using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class Category : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }

    public int? ParentCategoryId { get; private set; }

    public Category? ParentCategory { get; private set; }

    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    // Для EF Core
    protected Category() { }

    public Category(string name, string? description = null, string? imageUrl = null)
    {
        SetName(name);
        Description = description;
        ImageUrl = imageUrl;
        ParentCategoryId = null;
    }

    public void Rename(string newName)
    {
        SetName(newName);
    }

    public void UpdateDetails(string? description, string? imageUrl)
    {
        Description = description;
        ImageUrl = imageUrl;
    }

    public void SetParent(Category parent)
    {
        if (parent == null)
            throw new DomainException("Родительская категория не указана");

        if (parent.Id == Id)
            throw new DomainException("Категория не может быть родителем сама себе");

        if (parent.ParentCategoryId.HasValue)
            throw new DomainException("Максимальная вложенность категорий — 2 уровня");

        ParentCategory = parent;
        ParentCategoryId = parent.Id;
    }

    public void RemoveParent()
    {
        ParentCategory = null;
        ParentCategoryId = null;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Название категории обязательно");

        var trimmed = name.Trim();

        if (trimmed.Length > 100)
            throw new DomainException("Название категории не может быть длиннее 100 символов");

        Name = trimmed;
    }
}