using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Catalog.Entities;

namespace Backend.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedOnAdd();

        builder.Property(b => b.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(b => b.Name)
            .IsUnique()
            .HasDatabaseName("IX_Brands_Name");

        builder.Metadata
            .FindNavigation(nameof(Brand.Products))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(c => c.ImageUrl)
            .HasColumnName("ImageUrl")
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.ParentCategoryId)
            .HasColumnName("ParentCategoryId")
            .IsRequired(false);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Metadata
            .FindNavigation(nameof(Category.SubCategories))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("Types");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("IX_Types_Name");

        builder.Metadata
            .FindNavigation(nameof(ProductType.Products))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}