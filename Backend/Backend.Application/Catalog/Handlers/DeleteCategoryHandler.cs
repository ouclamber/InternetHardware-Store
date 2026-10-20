using MediatR;
using Backend.Application.Catalog.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Handlers;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category == null)
            throw new DomainException($"Категория с Id={request.CategoryId} не найдена");

        // Проверка: есть ли товары
        var hasProducts = await _categoryRepository.HasProductsAsync(
            request.CategoryId, cancellationToken);
        if (hasProducts)
            throw new DomainException(
                $"Нельзя удалить категорию '{category.Name}': в ней есть товары");

        // Проверка: есть ли подкатегории
        var hasSubCategories = await _categoryRepository.HasSubCategoriesAsync(
            request.CategoryId, cancellationToken);
        if (hasSubCategories)
            throw new DomainException(
                $"Нельзя удалить категорию '{category.Name}': у неё есть подкатегории");

        await _categoryRepository.DeleteAsync(category, cancellationToken);
    }
}