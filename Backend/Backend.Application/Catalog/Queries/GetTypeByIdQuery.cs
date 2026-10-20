using MediatR;
using Backend.Application.Catalog.DTOs;

namespace Backend.Application.Catalog.Queries;

public class GetTypeByIdQuery : IRequest<ProductTypeDto?>
{
    public int TypeId { get; set; }

    public GetTypeByIdQuery(int typeId)
    {
        TypeId = typeId;
    }
}