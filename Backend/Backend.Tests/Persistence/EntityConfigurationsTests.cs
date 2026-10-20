using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace Backend.Tests.Persistence;

public class EntityConfigurationsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public EntityConfigurationsTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void User_TableName_IsUsers()
    {
        var entity = _context.Model.FindEntityType(typeof(User));
        entity!.GetTableName().Should().Be("Users");
    }

    [Fact]
    public void Product_TableName_IsProducts()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        entity!.GetTableName().Should().Be("Products");
    }

    [Fact]
    public void Brand_TableName_IsBrands()
    {
        _context.Model.FindEntityType(typeof(Brand))!.GetTableName().Should().Be("Brands");
    }

    [Fact]
    public void Category_TableName_IsCategories()
    {
        _context.Model.FindEntityType(typeof(Category))!.GetTableName().Should().Be("Categories");
    }

    [Fact]
    public void ProductType_TableName_IsTypes()
    {
        _context.Model.FindEntityType(typeof(ProductType))!.GetTableName().Should().Be("Types");
    }

    [Fact]
    public void Cart_TableName_IsCarts()
    {
        _context.Model.FindEntityType(typeof(Cart))!.GetTableName().Should().Be("Carts");
    }

    [Fact]
    public void CartItem_TableName_IsCartItems()
    {
        _context.Model.FindEntityType(typeof(CartItem))!.GetTableName().Should().Be("CartItems");
    }

    [Fact]
    public void Order_TableName_IsOrders()
    {
        _context.Model.FindEntityType(typeof(Order))!.GetTableName().Should().Be("Orders");
    }

    [Fact]
    public void OrderItem_TableName_IsOrderItems()
    {
        _context.Model.FindEntityType(typeof(OrderItem))!.GetTableName().Should().Be("OrderItems");
    }

    [Fact]
    public void Review_TableName_IsReviews()
    {
        _context.Model.FindEntityType(typeof(Review))!.GetTableName().Should().Be("Reviews");
    }

    [Fact]
    public void ProductImage_TableName_IsProductImages()
    {
        _context.Model.FindEntityType(typeof(ProductImage))!.GetTableName().Should().Be("ProductImages");
    }

    [Fact]
    public void ProductAttribute_TableName_IsProductAttributes()
    {
        _context.Model.FindEntityType(typeof(ProductAttribute))!.GetTableName().Should().Be("ProductAttributes");
    }

    [Fact]
    public void ProductAttributeValue_TableName_IsProductAttributeValues()
    {
        _context.Model.FindEntityType(typeof(ProductAttributeValue))!.GetTableName().Should().Be("ProductAttributeValues");
    }

    [Fact]
    public void User_HasUniqueIndexOnUserName()
    {
        var entity = _context.Model.FindEntityType(typeof(User));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "UserName") && i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void Brand_HasUniqueIndexOnName()
    {
        var entity = _context.Model.FindEntityType(typeof(Brand));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Name") && i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void ProductType_HasUniqueIndexOnName()
    {
        var entity = _context.Model.FindEntityType(typeof(ProductType));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "Name") && i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void Cart_HasUniqueIndexOnUserId()
    {
        var entity = _context.Model.FindEntityType(typeof(Cart));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "UserId") && i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void Order_HasUniqueIndexOnOrderNumber()
    {
        var entity = _context.Model.FindEntityType(typeof(Order));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "OrderNumber") && i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void Review_HasUniqueIndexOnUserAndProduct()
    {
        var entity = _context.Model.FindEntityType(typeof(Review));
        var index = entity!.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == "UserId") &&
            i.Properties.Any(p => p.Name == "ProductId") &&
            i.IsUnique);

        index.Should().NotBeNull();
    }

    [Fact]
    public void Product_HasForeignKeyToBrand()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(Brand));

        fk.Should().NotBeNull();
    }

    [Fact]
    public void Product_HasForeignKeyToCategory()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(Category));

        fk.Should().NotBeNull();
    }

    [Fact]
    public void Product_HasForeignKeyToType()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(ProductType));

        fk.Should().NotBeNull();
    }

    [Fact]
    public void CartItem_HasForeignKeyToCart()
    {
        var entity = _context.Model.FindEntityType(typeof(CartItem));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(Cart));

        fk.Should().NotBeNull();
        fk!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void OrderItem_HasForeignKeyToOrder()
    {
        var entity = _context.Model.FindEntityType(typeof(OrderItem));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(Order));

        fk.Should().NotBeNull();
        fk!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void Review_HasForeignKeyToUser()
    {
        var entity = _context.Model.FindEntityType(typeof(Review));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(User));

        fk.Should().NotBeNull();
    }

    [Fact]
    public void Review_HasForeignKeyToProduct()
    {
        var entity = _context.Model.FindEntityType(typeof(Review));
        var fk = entity!.GetForeignKeys().FirstOrDefault(f =>
            f.PrincipalEntityType.ClrType == typeof(Product));

        fk.Should().NotBeNull();
    }

    [Fact]
    public void User_UserName_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(User));
        var prop = entity!.FindProperty("UserName");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void User_PasswordHash_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(User));
        var prop = entity!.FindProperty("PasswordHash");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void User_Role_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(User));
        var prop = entity!.FindProperty("Role");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Product_Name_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        var prop = entity!.FindProperty("Name");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Product_Price_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Product));
        var prop = entity!.FindProperty("Price");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Order_OrderNumber_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Order));
        var prop = entity!.FindProperty("OrderNumber");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Order_Status_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Order));
        var prop = entity!.FindProperty("Status");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Review_Rating_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Review));
        var prop = entity!.FindProperty("Rating");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void Review_Comment_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(Review));
        var prop = entity!.FindProperty("Comment");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void CartItem_Quantity_HasConverter()
    {
        var entity = _context.Model.FindEntityType(typeof(CartItem));
        var prop = entity!.FindProperty("Quantity");
        prop!.GetValueConverter().Should().NotBeNull();
    }

    [Fact]
    public void CartItem_TotalPrice_IsIgnored()
    {
        var entity = _context.Model.FindEntityType(typeof(CartItem));
        entity!.FindProperty("TotalPrice").Should().BeNull();
    }

    [Fact]
    public void OrderItem_TotalPrice_IsIgnored()
    {
        var entity = _context.Model.FindEntityType(typeof(OrderItem));
        entity!.FindProperty("TotalPrice").Should().BeNull();
    }

    [Fact]
    public void Context_HasDbSet_Users()
    {
        _context.Users.Should().NotBeNull();
    }

    [Fact]
    public void Context_HasDbSet_Products()
    {
        _context.Products.Should().NotBeNull();
    }

    [Fact]
    public void Context_HasDbSet_Orders()
    {
        _context.Orders.Should().NotBeNull();
    }

    [Fact]
    public void Context_HasDbSet_Reviews()
    {
        _context.Reviews.Should().NotBeNull();
    }
}