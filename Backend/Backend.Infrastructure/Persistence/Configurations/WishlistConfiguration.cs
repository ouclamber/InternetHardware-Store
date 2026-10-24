using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Users;
using Backend.Domain.Wishlist;

namespace Backend.Infrastructure.Persistence.Configurations;

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedOnAdd();

        builder.Property(w => w.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(w => w.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(w => w.AddedAt)
            .HasColumnName("AddedAt")
            .IsRequired();

        // Удаление пользователя или товара удаляет и запись избранного
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один товар — один раз в избранном пользователя
        builder.HasIndex(w => new { w.UserId, w.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_WishlistItems_User_Product");

        builder.HasIndex(w => w.ProductId)
            .HasDatabaseName("IX_WishlistItems_ProductId");
    }
}
