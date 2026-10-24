using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Catalog.Entities;

namespace Backend.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(i => i.ImageUrl)
            .HasColumnName("ImageUrl")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasColumnName("AltText")
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(i => i.IsMain)
            .HasColumnName("IsMain")
            .IsRequired();

        // ProductId → Product
        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Name)
            .HasColumnName("Name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.AttributeGroup)
            .HasColumnName("AttributeGroup")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(a => a.Unit)
            .HasColumnName("Unit")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.HasIndex(a => a.Name)
            .IsUnique()
            .HasDatabaseName("IX_ProductAttributes_Name");

        builder.Metadata
            .FindNavigation(nameof(ProductAttribute.Values))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("ProductAttributeValues");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(v => v.AttributeId)
            .HasColumnName("AttributeId")
            .IsRequired();

        builder.Property(v => v.Value)
            .HasColumnName("Value")
            .HasMaxLength(500)
            .IsRequired();

        // Product → Value
        builder.HasOne(v => v.Product)
            .WithMany(p => p.Values)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attribute → Value
        builder.HasOne(v => v.ProductAttributes)
            .WithMany(a => a.Values)
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Уникальность: (ProductId, AttributeId)
        builder.HasIndex(v => new { v.ProductId, v.AttributeId })
            .IsUnique()
            .HasDatabaseName("IX_ProductAttributeValues_Product_Attribute");
    }
}