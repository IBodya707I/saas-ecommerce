using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orders.Dal.Interfaces;
using Platform.EventBus.Events;

namespace Orders.Api.IntegrationEvents;

public class ProductPriceChangedHandler(IUnitOfWork uow, ILogger<ProductPriceChangedHandler> logger)
{
    public async Task HandleAsync(ProductPriceChangedEvent e, CancellationToken ct = default)
    {
        await uow.BeginTransactionAsync(ct: ct);
        try
        {
            if (!await uow.ProcessedEvents.TryAddAsync(e.EventId, ct))
            {
                logger.LogInformation("Duplicate event {EventId} skipped", e.EventId);
                await uow.RollbackAsync(ct);
                return;
            }

            await uow.Products.UpdateSnapshotAsync(e.ProductId, e.ProductName, e.NewPrice, ct);
            await uow.CommitAsync(ct);
            logger.LogInformation("Updated product {ProductId} price to {NewPrice} (Event {EventId}, CorrelationId: {CorrelationId})",
                e.ProductId, e.NewPrice, e.EventId, e.CorrelationId);
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(ct);
            logger.LogError(ex, "Failed to process ProductPriceChangedEvent {EventId} (CorrelationId: {CorrelationId}) for product {ProductId}",
                e.EventId, e.CorrelationId, e.ProductId);
            throw;
        }
    }
}
