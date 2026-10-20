using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;

namespace Backend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Id — автоинкремент
        builder.Property(u => u.Id)
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UserName)
            .HasConversion(
                userName => userName.Value,                    // VO → string
                value => UserName.Create(value))               // string → VO
            .HasColumnName("UserName")
            .HasMaxLength(50)
            .IsRequired();

        // Индекс на уникальность имени
        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        builder.Property(u => u.PasswordHash)
            .HasConversion(
                hash => hash.Value,
                value => PasswordHash.FromHash(value))
            .HasColumnName("PasswordHash")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasConversion(
                role => role.ToCode(),                          // enum → "User"/"Admin"
                code => RoleExtensions.FromCode(code))         // "User"/"Admin" → enum
            .HasColumnName("Role")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("UpdatedAt")
            .IsRequired(false);
    }
}