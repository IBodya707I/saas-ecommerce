namespace Reviews.WebApi.HealthChecks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Reviews.Infrastructure.Persistence;

public static class ReviewsHealthChecks
{
    public static IHealthChecksBuilder AddReviewsHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("redis")
            ?? "localhost:6379";

        return services.AddHealthChecks()
            .AddMongoDb(
                sp => sp.GetRequiredService<MongoDbContext>().Database.Client,
                name: "reviews-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database", "mongodb", "ready" },
                timeout: TimeSpan.FromSeconds(5))
            .AddRedis(
                redisConnectionString: redisConnectionString,
                name: "redis-cache",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "cache", "redis", "ready" },
                timeout: TimeSpan.FromSeconds(3));
    }
}
