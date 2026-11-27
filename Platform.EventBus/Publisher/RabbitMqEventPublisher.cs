using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.EventBus.Abstractions;
using Platform.EventBus.Diagnostics;
using Platform.EventBus.Events;
using Platform.EventBus.Serialization;
using RabbitMQ.Client;

namespace Platform.EventBus.Publisher;

public class RabbitMqEventPublisher(IConnection connection, ILogger<RabbitMqEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<T>(T @event, string exchange, string routingKey, CancellationToken ct = default)
        where T : IntegrationEvent
    {
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        
        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = @event.EventId.ToString(),
            Type = typeof(T).Name,
            Headers = new Dictionary<string, object?>
            {
                ["x-correlation-id"] = @event.CorrelationId
            }
        };

        TracePropagation.Inject(props);

        var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonDefaults.Options);
        
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: ct);

        logger.LogInformation("Published {EventType} {EventId} (CorrelationId: {CorrelationId}) to {Exchange}/{RoutingKey}",
            typeof(T).Name, @event.EventId, @event.CorrelationId, exchange, routingKey);
    }
}
