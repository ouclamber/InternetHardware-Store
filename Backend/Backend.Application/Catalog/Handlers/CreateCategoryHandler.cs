using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<int> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _categoryRepository.ExistsAsync(request.Name, cancellationToken);
        if (exists)
            throw new DomainException($"Категория '{request.Name}' уже существует");

        var category = new Category(request.Name, request.Description, request.ImageUrl);

        // Если есть родитель — установить
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(
                request.ParentCategoryId.Value, cancellationToken);

            if (parent == null)
                throw new DomainException(
                    $"Родительская категория с Id={request.ParentCategoryId} не найдена");

            category.SetParent(parent);
        }

        await _categoryRepository.AddAsync(category, cancellationToken);

        return category.Id;
    }
}