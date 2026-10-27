namespace Reviews.Infrastructure.Persistence;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reviews.Domain.Entities;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public IMongoCollection<Review> Reviews => Database.GetCollection<Review>("reviews");
    public IMongoCollection<StorefrontShowcase> StorefrontShowcases => Database.GetCollection<StorefrontShowcase>("storefront_showcases");
    public IMongoCollection<ProductDiscussion> ProductDiscussions => Database.GetCollection<ProductDiscussion>("product_discussions");

    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        var settings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
        var client = new MongoClient(settings);
        Database = client.GetDatabase(options.Value.DatabaseName);
    }
}
