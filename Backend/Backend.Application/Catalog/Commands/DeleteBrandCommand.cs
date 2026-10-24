using MediatR;

namespace Backend.Application.Catalog.Commands;

public class DeleteBrandCommand : IRequest
{
    public int BrandId { get; set; }

    public DeleteBrandCommand(int brandId)
    {
        BrandId = brandId;
    }
}