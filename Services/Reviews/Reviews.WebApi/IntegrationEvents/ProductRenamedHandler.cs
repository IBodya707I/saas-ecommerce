using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Platform.EventBus.Events;
using Reviews.Domain.Entities;
using Reviews.Infrastructure.Persistence;

namespace Reviews.WebApi.IntegrationEvents;

public class ProductRenamedHandler(
    MongoDbContext dbContext,
    IProcessedEventsStore processedEvents,
    ILogger<ProductRenamedHandler> logger)
{
    public async Task HandleAsync(ProductRenamedEvent e, CancellationToken ct = default)
    {
        if (!await processedEvents.TryAddAsync(e.EventId, ct))
        {
            logger.LogInformation("Duplicate event {EventId} skipped", e.EventId);
            return;
        }

        var _reviews = dbContext.Reviews;

        var filter = Builders<Review>.Filter.Eq("productId", e.ProductId.ToString())
                   | Builders<Review>.Filter.Eq("productId", e.ProductId)
                   | Builders<Review>.Filter.Eq(r => r.ProductName, e.OldName);

        var update = Builders<Review>.Update.Set(r => r.ProductName, e.ProductName);

        var result = await _reviews.UpdateManyAsync(filter, update, cancellationToken: ct);

        logger.LogInformation("Updated product name to '{NewName}' across {Count} reviews (Event {EventId}, CorrelationId: {CorrelationId})",
            e.ProductName, result.ModifiedCount, e.EventId, e.CorrelationId);
    }
}
