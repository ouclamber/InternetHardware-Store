using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using System.Text.Json;

namespace Backend.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.UserId)
            .HasColumnName("UserId")
            .IsRequired();

        // OrderNumber (VO → string)
        builder.Property(o => o.OrderNumber)
            .HasConversion(
                num => num.Value,
                value => OrderNumber.FromString(value))
            .HasColumnName("OrderNumber")
            .HasMaxLength(50)
            .IsRequired();

        // Status (enum → string)
        builder.Property(o => o.Status)
            .HasConversion(
                s => s.ToCode(),
                code => OrderStatusExtensions.FromCode(code))
            .HasColumnName("Status")
            .HasMaxLength(20)
            .IsRequired();

        // TotalAmount (Money → decimal)
        builder.Property(o => o.TotalAmount)
            .HasConversion(
                money => money.Amount,
                amount => Money.Rub(amount))
            .HasColumnName("TotalAmount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        var addressConverter = new ValueConverter<Address, string>(
            address => JsonSerializer.Serialize(address, (JsonSerializerOptions?)null),
            json => JsonSerializer.Deserialize<Address>(json, (JsonSerializerOptions?)null)!);

        builder.Property(o => o.ShippingAddress)
            .HasConversion(addressConverter)
            .HasColumnName("ShippingAddress")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(o => o.DeliveryMethod)
            .HasColumnName("DeliveryMethod")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(o => o.PaymentMethod)
            .HasColumnName("PaymentMethod")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(o => o.CustomerComment)
            .HasColumnName("Comment")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);

        builder.Property(o => o.PaidAt)
            .HasColumnName("PaidAt")
            .IsRequired(false);

        builder.Property(o => o.ShippedAt)
            .HasColumnName("ShippedAt")
            .IsRequired(false);

        builder.Property(o => o.DeliveredAt)
            .HasColumnName("DeliveredAt")
            .IsRequired(false);

        builder.Property(o => o.CancelledAt)
            .HasColumnName("CancelledAt")
            .IsRequired(false);

        builder.Metadata
            .FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(o => o.OrderNumber)
            .IsUnique()
            .HasDatabaseName("IX_Orders_OrderNumber");

        builder.HasIndex(o => o.UserId)
            .HasDatabaseName("IX_Orders_UserId");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(o => new { o.UserId, o.CreatedAt })
            .HasDatabaseName("IX_Orders_User_CreatedAt");
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.OrderId)
            .HasColumnName("OrderId")
            .IsRequired();

        builder.Property(i => i.ProductId)
            .HasColumnName("ProductId")
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasColumnName("ProductName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .HasColumnName("Quantity")
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasConversion(
                money => money.Amount,
                amount => Money.Rub(amount))
            .HasColumnName("UnitPrice")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Ignore(i => i.TotalPrice);

        builder.HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.OrderId)
            .HasDatabaseName("IX_OrderItems_OrderId");
    }
}