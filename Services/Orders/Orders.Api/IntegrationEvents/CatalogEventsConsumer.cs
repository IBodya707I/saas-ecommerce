using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.EventBus.Diagnostics;
using Platform.EventBus.Events;
using Platform.EventBus.Serialization;
using Platform.EventBus.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.Api.IntegrationEvents;

public class CatalogEventsConsumer(
    IConnection connection,
    IServiceScopeFactory scopes,
    ILogger<CatalogEventsConsumer> logger) : BackgroundService
{
    private const int MaxRetries = 3;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await RabbitMqTopology.DeclareTopologyAsync(_channel, stoppingToken);
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: RabbitMqTopology.OrdersQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("Started listening on queue '{Queue}'", RabbitMqTopology.OrdersQueue);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown
        }
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        using var activity = TracePropagation.StartConsumerActivity(ea, RabbitMqTopology.OrdersQueue);
        try
        {
            var eventType = ea.BasicProperties.Type;
            await using var scope = scopes.CreateAsyncScope();

            if (eventType == nameof(ProductPriceChangedEvent) || eventType == "ProductPriceChangedIntegrationEvent")
            {
                var @event = JsonSerializer.Deserialize<ProductPriceChangedEvent>(ea.Body.Span, JsonDefaults.Options)
                    ?? throw new JsonException("Empty or invalid ProductPriceChangedEvent payload");

                var handler = scope.ServiceProvider.GetRequiredService<ProductPriceChangedHandler>();
                await handler.HandleAsync(@event, ea.CancellationToken);
            }
            else if (eventType == nameof(ProductRenamedEvent) || eventType == "ProductRenamedIntegrationEvent")
            {
                var @event = JsonSerializer.Deserialize<ProductRenamedEvent>(ea.Body.Span, JsonDefaults.Options)
                    ?? throw new JsonException("Empty or invalid ProductRenamedEvent payload");

                var handler = scope.ServiceProvider.GetRequiredService<ProductRenamedHandler>();
                await handler.HandleAsync(@event, ea.CancellationToken);
            }
            else
            {
                logger.LogWarning("Unknown event type '{EventType}' on message {MessageId}, discarding",
                    eventType, ea.BasicProperties.MessageId);
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Malformed message {MessageId}, sending to DLQ", ea.BasicProperties.MessageId);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
        catch (Exception ex)
        {
            await RetryOrDeadLetterAsync(ea, ex);
        }
    }

    private async Task RetryOrDeadLetterAsync(BasicDeliverEventArgs ea, Exception ex)
    {
        var headers = ea.BasicProperties.Headers != null
            ? new Dictionary<string, object?>(ea.BasicProperties.Headers)
            : new Dictionary<string, object?>();

        var attempt = 0;
        if (headers.TryGetValue("x-retry-count", out var v) && v != null)
        {
            attempt = v is int i
                ? i
                : (v is byte[] bytes ? BitConverter.ToInt32(bytes) : Convert.ToInt32(v));
        }

        if (attempt >= MaxRetries)
        {
            logger.LogError(ex, "Message {MessageId} failed after {Attempts} attempts, routing to DLQ",
                ea.BasicProperties.MessageId, attempt);
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = ea.BasicProperties.ContentType ?? "application/json",
            MessageId = ea.BasicProperties.MessageId,
            Type = ea.BasicProperties.Type,
            Headers = new Dictionary<string, object?>(headers)
            {
                ["x-retry-count"] = attempt + 1
            }
        };

        await _channel!.BasicPublishAsync(
            exchange: ea.Exchange,
            routingKey: ea.RoutingKey,
            mandatory: false,
            basicProperties: props,
            body: ea.Body);

        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        logger.LogWarning(ex, "Message {MessageId} failed, retrying attempt {Attempt}/{MaxRetries}",
            ea.BasicProperties.MessageId, attempt + 1, MaxRetries);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            _channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
