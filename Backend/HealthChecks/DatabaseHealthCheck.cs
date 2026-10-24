using Microsoft.Extensions.Diagnostics.HealthChecks;
using Backend.Infrastructure.Persistence;

namespace Backend.HealthChecks;

/// <summary>
/// Проверка доступности PostgreSQL для /health/ready (без дополнительных NuGet-пакетов).
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _db;

    public DatabaseHealthCheck(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("PostgreSQL доступен")
                : HealthCheckResult.Unhealthy("Нет соединения с PostgreSQL");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Ошибка проверки PostgreSQL", ex);
        }
    }
}
