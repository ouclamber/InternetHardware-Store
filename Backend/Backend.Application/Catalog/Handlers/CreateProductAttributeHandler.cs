using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateProductAttributeHandler 
    : IRequestHandler<CreateProductAttributeCommand, int>
{
    private readonly IProductAttributeRepository _attributeRepository;

    public CreateProductAttributeHandler(IProductAttributeRepository attributeRepository)
    {
        _attributeRepository = attributeRepository;
    }

    public async Task<int> Handle(
        CreateProductAttributeCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _attributeRepository.ExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new DomainException($"Атрибут '{request.Name}' уже существует");

        var attribute = new ProductAttribute(request.Name, request.Group, request.Unit);

        await _attributeRepository.AddAsync(attribute, cancellationToken);

        return attribute.Id;
    }
}