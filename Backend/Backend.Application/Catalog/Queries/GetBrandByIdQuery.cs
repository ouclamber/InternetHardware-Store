using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetBrandByIdQuery : IRequest<BrandDto?>
{
    public int BrandId { get; set; }

    public GetBrandByIdQuery(int brandId)
    {
        BrandId = brandId;
    }
}