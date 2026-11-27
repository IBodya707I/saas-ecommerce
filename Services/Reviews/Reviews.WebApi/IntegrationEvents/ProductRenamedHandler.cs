using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Platform.EventBus.Events;
using Reviews.Application.Caching;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

namespace Reviews.WebApi.IntegrationEvents;

public class ProductRenamedHandler(
    MongoDbContext dbContext,
    IProcessedEventsStore processedEvents,
    IReviewsCacheService cache,
    ILogger<ProductRenamedHandler> logger)
{
    public async Task HandleAsync(ProductRenamedEvent e, CancellationToken ct = default)
    {
        if (!await processedEvents.TryAddAsync(e.EventId, ct))
        {
            logger.LogInformation("Duplicate event {EventId} skipped (CorrelationId: {CorrelationId})",
                e.EventId, e.CorrelationId);
            return;
        }

        long numericProductId = Math.Abs(BitConverter.ToInt64(e.ProductId.ToByteArray(), 0));

        var filter = Builders<Review>.Filter.Eq(r => r.ProductId, numericProductId)
                   | Builders<Review>.Filter.Eq(r => r.ProductName, e.OldName);

        var update = Builders<Review>.Update.Set(r => r.ProductName, e.ProductName);

        var result = await dbContext.Reviews.UpdateManyAsync(filter, update, cancellationToken: ct);

        await cache.IncrementVersionAsync($"reviews:ver:{numericProductId}", ct);
        await cache.IncrementVersionAsync("reviews:ver:all", ct);

        logger.LogInformation(
            "Updated product name to '{NewName}' across {Count} reviews (Event {EventId}, CorrelationId: {CorrelationId})",
            e.ProductName, result.ModifiedCount, e.EventId, e.CorrelationId);
    }
}
