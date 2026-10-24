using Backend.Domain.Shared;

namespace Backend.Domain.Catalog.Entities;

public class ProductImage : Entity
{
    public int ProductId { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public bool IsMain { get; private set; }

    public Product? Product { get; private set; }

    // Для EF Core
    protected ProductImage() { }

    internal ProductImage(string imageUrl, string? altText = null, bool isMain = false)
    {
        SetImageUrl(imageUrl);
        AltText = altText;
        IsMain = isMain;
    }

    public void Update(string imageUrl, string? altText)
    {
        SetImageUrl(imageUrl);
        AltText = altText;
    }

    internal void MakeMain()
    {
        IsMain = true;
    }

    internal void RemoveMainStatus()
    {
        IsMain = false;
    }

    private void SetImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new DomainException("URL изображения обязателен");

        var trimmed = imageUrl.Trim();

        if (trimmed.Length > 500)
            throw new DomainException("URL изображения не может быть длиннее 500 символов");

        ImageUrl = trimmed;
    }
}