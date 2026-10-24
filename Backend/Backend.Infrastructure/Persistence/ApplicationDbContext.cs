using Microsoft.EntityFrameworkCore;
using Npgsql;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Sales.CartAggregate;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Reviews;
using Backend.Domain.Wishlist;
using Backend.Infrastructure.Persistence.Converters;

namespace Backend.Infrastructure.Persistence;

/// <summary>
/// DbContext — он же Unit of Work: все изменения отслеживаемых агрегатов
/// фиксируются одной транзакцией в SaveChangesAsync
/// (IUnitOfWork.SaveChangesAsync реализуется унаследованным DbContext.SaveChangesAsync).
/// </summary>
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public const string ConcurrencyConflictMessage =
        "Данные изменились, пока выполнялся запрос (например, товар только что раскупили). " +
        "Обновите страницу и повторите попытку.";

    public const string UniqueConflictMessage =
        "Такая запись уже существует.";

    private readonly IEncryptionService? _encryption;

    /// <param name="encryption">
    /// Сервис шифрования персональных данных. В приложении внедряется из DI;
    /// в юнит-тестах может отсутствовать — тогда данные хранятся без шифрования.
    /// Модель EF кешируется на тип контекста, поэтому сервис должен быть синглтоном.
    /// </param>
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IEncryptionService? encryption = null)
        : base(options)
    {
        _encryption = encryption;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductType> Types => Set<ProductType>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Свойство nullable, конвертер — для ненулевого CustomerContact: EF не передаёт null в конвертеры
        // (NULL в БД остаётся NULL), поэтому предупреждение о nullability здесь не актуально.
#pragma warning disable CS8620
        modelBuilder.Entity<Order>()
            .Property(o => o.Contact)
            .HasConversion(new CustomerContactConverter(_encryption));
#pragma warning restore CS8620
    }

    /// <summary>
    /// Ошибки конкурентного доступа и нарушения уникальности переводятся в доменный
    /// <see cref="ConflictException"/> (HTTP 409) — веб-слой не зависит от EF Core.
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException(ConcurrencyConflictMessage, ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ConflictException(UniqueConflictMessage, ex);
        }
    }
}
