namespace Reviews.Infrastructure.Seeding;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

public class IndexCreationService : IIndexCreationService, IHostedService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<IndexCreationService> _logger;

    public IndexCreationService(MongoDbContext context, ILogger<IndexCreationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateIndexesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating MongoDB indexes for Reviews service...");

            var reviewKeys = Builders<Review>.IndexKeys;
            var reviewModels = new[]
            {
                new CreateIndexModel<Review>(
                    reviewKeys.Ascending(r => r.ProductId).Descending(r => r.CreatedAt),
                    new CreateIndexOptions { Name = "idx_reviews_product_createdat" }),

                new CreateIndexModel<Review>(
                    reviewKeys.Text(r => r.Text),
                    new CreateIndexOptions { Name = "idx_reviews_text" }),

                new CreateIndexModel<Review>(
                    reviewKeys.Ascending(r => r.ProductId).Ascending("author.userId"),
                    new CreateIndexOptions { Unique = true, Name = "idx_reviews_product_author_unique" })
            };
            await _context.Reviews.Indexes.CreateManyAsync(reviewModels, ct);

            var showcaseKeys = Builders<StorefrontShowcase>.IndexKeys;
            var showcaseModels = new[]
            {
                new CreateIndexModel<StorefrontShowcase>(
                    showcaseKeys.Ascending(s => s.StoreId),
                    new CreateIndexOptions { Unique = true, Name = "idx_showcases_storeid_unique" }),
                new CreateIndexModel<StorefrontShowcase>(
                    showcaseKeys.Ascending(s => s.Slug),
                    new CreateIndexOptions { Unique = true, Name = "idx_showcases_slug_unique" })
            };
            await _context.StorefrontShowcases.Indexes.CreateManyAsync(showcaseModels, ct);

            _logger.LogInformation("MongoDB indexes created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MongoDB indexes.");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateIndexesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
