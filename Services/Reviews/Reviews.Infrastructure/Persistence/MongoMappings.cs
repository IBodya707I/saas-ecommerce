namespace Reviews.Infrastructure.Persistence;

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Reviews.Domain.Common;
using Reviews.Domain.Entities;
using Reviews.Domain.ValueObjects;

public static class MongoMappings
{
    private static bool _isRegistered;
    private static readonly object LockObj = new();

    public static void Register()
    {
        if (_isRegistered) return;

        lock (LockObj)
        {
            if (_isRegistered) return;

            var conventionPack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("ReviewsConventions", conventionPack, _ => true);

            if (!BsonClassMap.IsClassMapRegistered(typeof(Author)))
            {
                BsonClassMap.RegisterClassMap<Author>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(a => a.UserId);
                    cm.MapProperty(a => a.DisplayName);
                    cm.MapCreator(a => new Author(a.UserId, a.DisplayName));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Rating)))
            {
                BsonClassMap.RegisterClassMap<Rating>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(r => r.Value);
                    cm.MapCreator(r => new Rating(r.Value));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(DeliveryRating)))
            {
                BsonClassMap.RegisterClassMap<DeliveryRating>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(r => r.Value);
                    cm.MapCreator(r => new DeliveryRating(r.Value));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Branding)))
            {
                BsonClassMap.RegisterClassMap<Branding>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(b => b.Color);
                    cm.MapProperty(b => b.FontFamily);
                    cm.MapProperty(b => b.LogoUrl);
                    cm.MapCreator(b => new Branding(b.Color, b.FontFamily, b.LogoUrl));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
            {
                BsonClassMap.RegisterClassMap<Comment>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(c => c.Id);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Review)))
            {
                BsonClassMap.RegisterClassMap<Review>(cm =>
                {
                    cm.MapMember(r => r.ProductId);
                    cm.MapMember(r => r.ProductName);
                    cm.MapMember(r => r.Author);
                    cm.MapMember(r => r.Rating);
                    cm.MapMember(r => r.Text);
                    cm.MapField("_comments").SetElementName("comments");
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(StorefrontShowcase)))
            {
                BsonClassMap.RegisterClassMap<StorefrontShowcase>(cm =>
                {
                    cm.MapMember(s => s.StoreId);
                    cm.MapMember(s => s.Slug);
                    cm.MapMember(s => s.StoreName);
                    cm.MapMember(s => s.Branding);
                    cm.MapMember(s => s.WorkSchedule);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(ProductDiscussion)))
            {
                BsonClassMap.RegisterClassMap<ProductDiscussion>(cm =>
                {
                    cm.MapMember(d => d.ProductId);
                    cm.MapMember(d => d.Title);
                    cm.MapMember(d => d.Status);
                    cm.MapMember(d => d.MessageCount);
                });
            }

            _isRegistered = true;
        }
    }
}
