using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Platform.EventBus.Consumer;
using Platform.EventBus.Events;
using Platform.EventBus.Serialization;
using Platform.EventBus.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.Api.IntegrationEvents;

public class CatalogEventsConsumer(
    IConnection connection,
    IServiceScopeFactory scopes,
    ILogger<CatalogEventsConsumer> logger)
    : RabbitMqConsumerBase(connection, scopes, logger, RabbitMqTopology.OrdersQueue)
{
    protected override async Task ProcessMessageAsync(
        BasicDeliverEventArgs ea,
        AsyncServiceScope scope,
        string? correlationId,
        CancellationToken ct)
    {
        var eventType = ea.BasicProperties.Type;

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new JsonException("Message type header is missing or empty");
        }

        if (eventType == nameof(ProductPriceChangedEvent) || eventType == "ProductPriceChangedIntegrationEvent")
        {
            var @event = JsonSerializer.Deserialize<ProductPriceChangedEvent>(ea.Body.Span, JsonDefaults.Options)
                ?? throw new JsonException("Empty or invalid ProductPriceChangedEvent payload");

            var handler = scope.ServiceProvider.GetRequiredService<ProductPriceChangedHandler>();
            await handler.HandleAsync(@event, ct);
        }
        else if (eventType == nameof(ProductRenamedEvent) || eventType == "ProductRenamedIntegrationEvent")
        {
            var @event = JsonSerializer.Deserialize<ProductRenamedEvent>(ea.Body.Span, JsonDefaults.Options)
                ?? throw new JsonException("Empty or invalid ProductRenamedEvent payload");

            var handler = scope.ServiceProvider.GetRequiredService<ProductRenamedHandler>();
            await handler.HandleAsync(@event, ct);
        }
        else
        {
            throw new JsonException($"Unknown or malformed event type '{eventType}' for orders sync");
        }
    }
}
