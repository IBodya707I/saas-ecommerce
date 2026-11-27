using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Reviews.Infrastructure.Persistence;

public class MongoProcessedEventsStore : IProcessedEventsStore
{
    private readonly IMongoCollection<ProcessedEventDocument> _collection;

    public MongoProcessedEventsStore(MongoDbContext context)
    {
        _collection = context.ProcessedEvents;
    }

    public async Task<bool> TryAddAsync(Guid eventId, CancellationToken ct = default)
    {
        try
        {
            await _collection.InsertOneAsync(
                new ProcessedEventDocument
                {
                    EventId = eventId.ToString(),
                    ProcessedAt = DateTime.UtcNow
                },
                cancellationToken: ct);

            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }
}
