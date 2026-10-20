using MediatR;

namespace Backend.Application.Catalog.Commands;

public class UpdateBrandCommand : IRequest
{
    public int BrandId { get; set; }
    public string NewName { get; set; } = string.Empty;

    public UpdateBrandCommand(int brandId, string newName)
    {
        BrandId = brandId;
        NewName = newName;
    }
}