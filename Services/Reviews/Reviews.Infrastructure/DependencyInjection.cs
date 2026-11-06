namespace Reviews.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reviews.Application.Common.Interfaces;
using Reviews.Domain.Interfaces;
using Reviews.Infrastructure.Persistence;
using Reviews.Infrastructure.Queries;
using Reviews.Infrastructure.Repositories;
using Reviews.Infrastructure.Seeding;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        MongoMappings.Register();

        services.Configure<MongoDbOptions>(options =>
        {
            configuration.GetSection(MongoDbOptions.SectionName).Bind(options);

            // Підтримка Aspire Service Discovery & ConnectionString
            var connectionString = configuration.GetConnectionString("showcase-db")
                ?? configuration.GetConnectionString("showcase_db");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.ConnectionString = connectionString;
            }
        });

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IStorefrontRepository, StorefrontRepository>();
        services.AddScoped<IReviewQueries, ReviewQueries>();
        services.AddScoped<IStorefrontQueries, StorefrontQueries>();

        services.AddScoped<IDataSeeder, ReviewSeeder>();
        services.AddSingleton<IIndexCreationService, IndexCreationService>();
        services.AddHostedService(sp => (IndexCreationService)sp.GetRequiredService<IIndexCreationService>());

        return services;
    }
}
