using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category == null)
            return null;

        return CategoryMapper.MapToDto(category);
    }
}