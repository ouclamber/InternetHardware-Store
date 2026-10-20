using Backend.Domain.Shared;
using Backend.Domain.Catalog.ValueObjects;

namespace Backend.Domain.Catalog.Entities;

public class Product : Entity, IAggregateRoot
{
    public ProductName Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public int BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    public int CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public int TypeId { get; private set; }
    public ProductType? Type { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<ProductAttributeValue> _values = new();
    public IReadOnlyCollection<ProductAttributeValue> Values => _values.AsReadOnly();

    // Для EF Core
    protected Product() { }

    public Product(
        ProductName name,
        Money price,
        int brandId,
        int categoryId,
        int typeId,
        string? description = null)
    {
        SetName(name);
        SetPrice(price);
        SetBrand(brandId);
        SetCategory(categoryId);
        SetType(typeId);
        Description = description?.Trim() ?? string.Empty;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Rename(ProductName newName)
    {
        SetName(newName);
        MarkAsUpdated();
    }

    public void UpdatePrice(Money newPrice)
    {
        SetPrice(newPrice);
        MarkAsUpdated();
    }

    public void UpdateDescription(string? newDescription)
    {
        Description = newDescription?.Trim() ?? string.Empty;
        MarkAsUpdated();
    }

    public void ChangeBrand(int newBrandId)
    {
        SetBrand(newBrandId);
        MarkAsUpdated();
    }

    public void ChangeCategory(int newCategoryId)
    {
        SetCategory(newCategoryId);
        MarkAsUpdated();
    }

    public void ChangeType(int newTypeId)
    {
        SetType(newTypeId);
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException($"Товар '{Name.Value}' уже деактивирован");

        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException($"Товар '{Name.Value}' уже активен");

        IsActive = true;
        MarkAsUpdated();
    }

    public void AddImage(string imageUrl, string? altText = null, bool isMain = false)
    {
        if (_images.Count >= 10)
            throw new DomainException("У товара может быть не более 10 изображений");

        if (isMain)
        {
            // Убрать "главное" у всех существующих
            foreach (var img in _images)
                img.RemoveMainStatus();
        }

        var image = new ProductImage(imageUrl, altText, isMain);
        _images.Add(image);
        MarkAsUpdated();
    }

    public void MakeImageMain(int imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new DomainException($"Изображение с Id={imageId} не найдено");

        foreach (var img in _images)
            img.RemoveMainStatus();

        image.MakeMain();
        MarkAsUpdated();
    }

    public void RemoveImage(int imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image == null)
            throw new DomainException($"Изображение с Id={imageId} не найдено");

        _images.Remove(image);

        // Если удалили главное — назначить первое главным
        if (image.IsMain && _images.Any())
            _images.First().MakeMain();

        MarkAsUpdated();
    }

    public void AddAttributeValue(int attributeId, string value)
    {
        var existing = _values.FirstOrDefault(v => v.AttributeId == attributeId);
        if (existing != null)
            throw new DomainException($"Атрибут с Id={attributeId} уже добавлен к товару");

        var attrValue = new ProductAttributeValue(Id, attributeId, value);
        _values.Add(attrValue);
        MarkAsUpdated();
    }

    public void UpdateAttributeValue(int attributeId, string newValue)
    {
        var existing = _values.FirstOrDefault(v => v.AttributeId == attributeId);
        if (existing == null)
            throw new DomainException($"Атрибут с Id={attributeId} не найден у товара");

        existing.UpdateValue(newValue);
        MarkAsUpdated();
    }

    public void RemoveAttributeValue(int attributeId)
    {
        var existing = _values.FirstOrDefault(v => v.AttributeId == attributeId);
        if (existing == null)
            throw new DomainException($"Атрибут с Id={attributeId} не найден у товара");

        _values.Remove(existing);
        MarkAsUpdated();
    }

    private void SetName(ProductName name)
    {
        Name = name ?? throw new DomainException("Название товара обязательно");
    }

    private void SetPrice(Money price)
    {
        if (price == null)
            throw new DomainException("Цена обязательна");

        if (price.IsZero)
            throw new DomainException("Цена должна быть больше 0");

        Price = price;
    }

    private void SetBrand(int brandId)
    {
        if (brandId <= 0)
            throw new DomainException("Id бренда обязателен");

        BrandId = brandId;
    }

    private void SetCategory(int categoryId)
    {
        if (categoryId <= 0)
            throw new DomainException("Id категории обязателен");

        CategoryId = categoryId;
    }

    private void SetType(int typeId)
    {
        if (typeId <= 0)
            throw new DomainException("Id типа обязателен");

        TypeId = typeId;
    }

    private void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}