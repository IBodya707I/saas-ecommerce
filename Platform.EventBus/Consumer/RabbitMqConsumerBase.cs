using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.EventBus.Diagnostics;
using Platform.EventBus.Topology;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Platform.EventBus.Consumer;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    protected const int MaxRetries = 3;
    protected readonly IConnection Connection;
    protected readonly IServiceScopeFactory Scopes;
    protected readonly ILogger Logger;
    protected readonly string QueueName;
    protected IChannel? Channel;

    protected RabbitMqConsumerBase(
        IConnection connection,
        IServiceScopeFactory scopes,
        ILogger logger,
        string queueName)
    {
        Connection = connection;
        Scopes = scopes;
        Logger = logger;
        QueueName = queueName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Channel = await Connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await RabbitMqTopology.DeclareTopologyAsync(Channel, stoppingToken);
        await Channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await Channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        Logger.LogInformation("Started listening on queue '{Queue}'", QueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    protected virtual async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        using var activity = TracePropagation.StartConsumerActivity(ea, QueueName);
        var correlationId = ExtractCorrelationId(ea);

        using var scope = Logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["MessageId"] = ea.BasicProperties.MessageId
        });

        try
        {
            await using var serviceScope = Scopes.CreateAsyncScope();
            await ProcessMessageAsync(ea, serviceScope, correlationId, ea.CancellationToken);
            await Channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Malformed message {MessageId} (CorrelationId: {CorrelationId}), sending to DLQ",
                ea.BasicProperties.MessageId, correlationId);
            await Channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
        catch (Exception ex)
        {
            await RetryOrDeadLetterAsync(ea, ex, correlationId);
        }
    }

    protected abstract Task ProcessMessageAsync(
        BasicDeliverEventArgs ea,
        AsyncServiceScope scope,
        string? correlationId,
        CancellationToken ct);

    protected virtual async Task RetryOrDeadLetterAsync(BasicDeliverEventArgs ea, Exception ex, string? correlationId)
    {
        var headers = ea.BasicProperties.Headers != null
            ? new Dictionary<string, object?>(ea.BasicProperties.Headers)
            : new Dictionary<string, object?>();

        var attempt = 0;
        if (headers.TryGetValue("x-retry-count", out var v) && v != null)
        {
            attempt = ParseRetryCount(v);
        }

        if (attempt >= MaxRetries)
        {
            Logger.LogError(ex, "Message {MessageId} (CorrelationId: {CorrelationId}) failed after {Attempts} attempts, routing to DLQ",
                ea.BasicProperties.MessageId, correlationId, attempt);
            await Channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
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
                ["x-retry-count"] = attempt + 1,
                ["x-correlation-id"] = correlationId
            }
        };

        await Channel!.BasicPublishAsync(
            exchange: ea.Exchange,
            routingKey: ea.RoutingKey,
            mandatory: false,
            basicProperties: props,
            body: ea.Body);

        await Channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        Logger.LogWarning(ex, "Message {MessageId} (CorrelationId: {CorrelationId}) retry {Attempt}/{MaxRetries}",
            ea.BasicProperties.MessageId, correlationId, attempt + 1, MaxRetries);
    }

    protected static string? ExtractCorrelationId(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-correlation-id", out var val) && val != null)
        {
            if (val is byte[] bytes) return Encoding.UTF8.GetString(bytes);
            return val.ToString();
        }
        return ea.BasicProperties.CorrelationId;
    }

    protected static int ParseRetryCount(object value)
    {
        if (value is int i) return i;
        if (value is long l) return (int)l;
        if (value is byte[] bytes)
        {
            var str = Encoding.UTF8.GetString(bytes);
            if (int.TryParse(str, out var parsed)) return parsed;
            if (bytes.Length == 4) return BitConverter.ToInt32(bytes, 0);
            return 0;
        }
        if (int.TryParse(value.ToString(), out var parsedStr)) return parsedStr;
        return 0;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Channel != null)
        {
            await Channel.CloseAsync(cancellationToken);
            Channel.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
