using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orders.Dal.Interfaces;
using Platform.EventBus.Events;

namespace Orders.Api.IntegrationEvents;

public class ProductRenamedHandler(IUnitOfWork uow, ILogger<ProductRenamedHandler> logger)
{
    public async Task HandleAsync(ProductRenamedEvent e, CancellationToken ct = default)
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

            await uow.Products.UpdateNameSnapshotAsync(e.ProductId, e.NewName, ct);
            await uow.CommitAsync(ct);
            logger.LogInformation("Updated product {ProductId} name to '{NewName}' (Event {EventId})",
                e.ProductId, e.NewName, e.EventId);
        }
        catch (Exception ex)
        {
            await uow.RollbackAsync(ct);
            logger.LogError(ex, "Failed to process ProductRenamedEvent {EventId} for product {ProductId}",
                e.EventId, e.ProductId);
            throw;
        }
    }
}
