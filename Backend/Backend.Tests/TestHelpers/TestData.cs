using System.Reflection;
using Backend.Application.Catalog.Handlers;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using Moq;

namespace Backend.Tests.TestHelpers;

/// <summary>
/// Общие фабрики тестовых данных.
/// </summary>
public static class TestData
{
    public static T WithId<T>(this T entity, int id) where T : Entity
    {
        typeof(Entity).GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(entity, id);
        return entity;
    }

    public static Product Product(int id = 1, string name = "Test product", decimal price = 1000m,
        int stock = 100, bool active = true)
    {
        var product = new Product(ProductName.Create(name), Money.Rub(price), 1, 1, 1, "desc", stock).WithId(id);
        if (!active)
            product.Deactivate();
        return product;
    }

    public static User User(int id = 1, string name = "testuser", Role role = Role.User) =>
        new User(UserName.Create(name), PasswordHash.FromHash(new string('a', 60)), role).WithId(id);

    /// <summary>
    /// CreateProductHandler, у которого бренд/категория/тип с любым Id существуют.
    /// </summary>
    public static CreateProductHandler CreateProductHandler(IProductRepository products)
    {
        var (brands, categories, types) = ExistingReferences();
        return new CreateProductHandler(products, brands.Object, categories.Object, types.Object);
    }

    public static (Mock<IBrandRepository> Brands, Mock<ICategoryRepository> Categories, Mock<ITypeRepository> Types)
        ExistingReferences()
    {
        var brands = new Mock<IBrandRepository>();
        var categories = new Mock<ICategoryRepository>();
        var types = new Mock<ITypeRepository>();

        brands.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new Brand("Brand"));
        categories.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Category("Category"));
        types.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new ProductType("Type"));

        return (brands, categories, types);
    }
}
