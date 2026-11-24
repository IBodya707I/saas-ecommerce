namespace Orders.Api.HealthChecks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class OrdersHealthChecks
{
    public static IHealthChecksBuilder AddOrdersHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("OrdersDb")
            ?? configuration.GetConnectionString("orders-db")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("OrdersDb connection string is missing");

        var redisConnectionString = configuration.GetConnectionString("redis")
            ?? "localhost:6379";

        return services.AddHealthChecks()
            .AddNpgSql(
                connectionString: dbConnectionString,
                name: "orders-db",
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
