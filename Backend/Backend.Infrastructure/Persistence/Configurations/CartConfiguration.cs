using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;   

namespace Backend.Infrastructure.Persistence.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        // Один User → одна Cart
        builder.HasIndex(c => c.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Carts_UserId");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);

        // Коллекция Items — через backing field
        builder.Metadata
            .FindNavigation(nameof(Cart.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.CartId)
            .HasColumnName("CartId")
            .IsRequired();

        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_CartItems_ProductId");

        builder.Property(i => i.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        // Quantity (VO → int)
        builder.Property(i => i.Quantity)
            .HasConversion(
                q => q.Value,
                v => Quantity.Of(v))
            .HasColumnName("Quantity")
            .IsRequired();

        // UnitPrice (Money → decimal)
        builder.Property(i => i.UnitPrice)
            .HasConversion(
                money => money.Amount,
                amount => Money.Rub(amount))
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // TotalPrice — computed, не сохраняем
        builder.Ignore(i => i.TotalPrice);

        // Cart 1 → N CartItem
        builder.HasOne(i => i.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product N → 1
        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Уникальность: (CartId, ProductId)
        builder.HasIndex(i => new { i.CartId, i.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_CartItems_Cart_Product");
    }
}