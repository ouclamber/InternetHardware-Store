using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Persistence;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Контекст с настройками как в приложении: запросы по умолчанию без отслеживания.
    /// Для сценариев «один HTTP-запрос = один контекст» создавайте несколько контекстов
    /// с одним и тем же <paramref name="dbName"/>.
    /// </summary>
    public static ApplicationDbContext CreateLikeProduction(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;

        return new ApplicationDbContext(options);
    }

    public static ApplicationDbContext CreateInMemory(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ApplicationDbContext(options);
    }
}