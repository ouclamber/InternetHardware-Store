using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasConversion(
                name => name.Value,
                value => ProductName.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasConversion(
                money => money.Amount,                         // Money → decimal
                amount => Money.Rub(amount))                   // decimal → Money
            .HasColumnName("Price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("Description")
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(p => p.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);

        builder.Property(p => p.BrandId)
            .HasColumnName("BrandId")
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasColumnName("CategoryId")
            .IsRequired();

        builder.Property(p => p.TypeId)
            .HasColumnName("TypeId")
            .IsRequired();

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Type)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Metadata
            .FindNavigation(nameof(Product.Images))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Product.Values))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Индексы
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => new { p.CategoryId, p.IsActive })
            .HasDatabaseName("IX_Products_Category_Active");

        builder.HasIndex(p => p.BrandId)
            .HasDatabaseName("IX_Products_BrandId");

        builder.HasIndex(p => p.TypeId)
            .HasDatabaseName("IX_Products_TypeId");

        builder.HasIndex(p => new { p.IsActive, p.Id })
            .HasDatabaseName("IX_Products_Active_Id");
    }
}