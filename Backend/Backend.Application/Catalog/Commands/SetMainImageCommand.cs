using MediatR;

namespace Backend.Application.Catalog.Commands;

public class SetMainImageCommand : IRequest
{
    public int ProductId { get; set; }
    public int ImageId { get; set; }

    public SetMainImageCommand(int productId, int imageId)
    {
        ProductId = productId;
        ImageId = imageId;
    }
}