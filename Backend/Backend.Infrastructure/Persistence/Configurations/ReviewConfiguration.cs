using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.ValueObjects;

namespace Backend.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        builder.Property(r => r.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(r => r.Rating)
            .HasConversion(
                rating => rating.Value,
                value => Rating.Of(value))
            .HasColumnName("Rating")
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasConversion(
                comment => comment.Value,
                value => Comment.Create(value))
            .HasColumnName("Comment")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion(
                s => s.ToCode(),
                code => ReviewStatusExtensions.FromCode(code))
            .HasColumnName("Status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);

        builder.Property(r => r.ModeratedAt)
            .HasColumnName("ModeratedAt")
            .IsRequired(false);

        builder.Property(r => r.ModeratedByUserId)
            .HasColumnName("ModeratedByUserId")
            .IsRequired(false);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.UserId, r.ProductId })
            .IsUnique()
            .HasDatabaseName("IX_Reviews_User_Product");

        builder.HasIndex(r => new { r.ProductId, r.Status })
            .HasDatabaseName("IX_Reviews_Product_Status");

        builder.HasIndex(r => r.Status)
            .HasDatabaseName("IX_Reviews_Status");
    }
}