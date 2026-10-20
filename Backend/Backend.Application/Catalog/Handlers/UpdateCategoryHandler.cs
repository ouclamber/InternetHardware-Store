using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category == null)
            throw new DomainException($"Категория с Id={request.CategoryId} не найдена");

        // Проверка: не занято ли новое имя
        if (category.Name != request.Name)
        {
            var exists = await _categoryRepository.ExistsAsync(request.Name, cancellationToken);
            if (exists)
                throw new DomainException($"Категория '{request.Name}' уже существует");
        }

        // Бизнес-методы
        category.Rename(request.Name);
        category.UpdateDetails(request.Description, request.ImageUrl);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
    }
}