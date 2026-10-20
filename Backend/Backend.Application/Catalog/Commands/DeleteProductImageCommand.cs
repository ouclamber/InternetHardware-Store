using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeleteProductImageCommand : IRequest
{
    public int ProductId { get; set; }
    public int ImageId { get; set; }

    public DeleteProductImageCommand(int productId, int imageId)
    {
        ProductId = productId;
        ImageId = imageId;
    }
}