using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Catalog.Services;

/// <summary>
/// Проверка ссылок товара на бренд/категорию/тип до сохранения.
/// Без неё несуществующий Id приводил к нарушению внешнего ключа в БД и ответу 500.
/// </summary>
internal static class CatalogReferenceValidator
{
    public static async Task EnsureExistAsync(
        IBrandRepository brands,
        ICategoryRepository categories,
        ITypeRepository types,
        int brandId,
        int categoryId,
        int typeId,
        CancellationToken ct)
    {
        if (await brands.GetByIdAsync(brandId, ct) == null)
            throw new DomainException($"Бренд с Id={brandId} не найден");

        if (await categories.GetByIdAsync(categoryId, ct) == null)
            throw new DomainException($"Категория с Id={categoryId} не найдена");

        if (await types.GetByIdAsync(typeId, ct) == null)
            throw new DomainException($"Тип с Id={typeId} не найден");
    }
}
