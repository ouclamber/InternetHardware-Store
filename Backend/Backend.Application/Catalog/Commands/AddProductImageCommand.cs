using MediatR;

namespace Backend.Application.Catalog.Commands;

public class AddProductImageCommand : IRequest<int>
{
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }

    public AddProductImageCommand(
        int productId,
        string imageUrl,
        string? altText = null,
        bool isMain = false)
    {
        ProductId = productId;
        ImageUrl = imageUrl;
        AltText = altText;
        IsMain = isMain;
    }
}