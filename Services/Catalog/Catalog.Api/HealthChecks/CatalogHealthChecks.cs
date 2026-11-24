namespace Catalog.Api.HealthChecks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class CatalogHealthChecks
{
    public static IHealthChecksBuilder AddCatalogHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("CatalogDb")
            ?? configuration.GetConnectionString("catalog-db")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Port=5432;Database=catalog_db;Username=postgres;Password=postgres;";

        var redisConnectionString = configuration.GetConnectionString("redis")
            ?? "localhost:6379";

        return services.AddHealthChecks()
            .AddNpgSql(
                connectionString: dbConnectionString,
                name: "catalog-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "sql", "ready" },
                timeout: TimeSpan.FromSeconds(5))
            .AddRedis(
                redisConnectionString: redisConnectionString,
                name: "redis-cache",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "redis", "ready" },
                timeout: TimeSpan.FromSeconds(3));
    }
}
