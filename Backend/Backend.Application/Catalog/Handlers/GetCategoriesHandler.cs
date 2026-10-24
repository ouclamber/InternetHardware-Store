using MediatR;
using Backend.Application.Catalog.DTOs;
using Backend.Application.Catalog.Mappers;
using Backend.Application.Catalog.Queries;
using Backend.Domain.Catalog.Repositories;

namespace Backend.Application.Catalog.Handlers;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = request.RootOnly
            ? await _categoryRepository.GetRootAsync(cancellationToken)
            : await _categoryRepository.GetAllAsync(cancellationToken);

        var counts = await _categoryRepository.GetActiveProductCountsAsync(cancellationToken)
                     ?? new Dictionary<int, int>();

        return categories
            .Select(category =>
            {
                var dto = CategoryMapper.MapToDto(category);
                dto.ProductCount = counts.TryGetValue(category.Id, out var count) ? count : 0;
                return dto;
            })
            .ToList();
    }
}