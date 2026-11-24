using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Platform.EventBus.Topology;

public static class RabbitMqTopology
{
    public const string CatalogExchange = "catalog-events";
    public const string CatalogDlx = "catalog-events.dlx";
    
    public const string OrdersQueue = "orders.catalog-sync";
    public const string OrdersDlq = "orders.catalog-sync.dlq";
    
    public const string ReviewsQueue = "reviews.catalog-sync";
    public const string ReviewsDlq = "reviews.catalog-sync.dlq";

    public const string ProductRoutingKeyPattern = "product.*";
    public const string ProductPriceChangedRoutingKey = "product.price_changed";
    public const string ProductRenamedRoutingKey = "product.renamed";

    public static async Task DeclareTopologyAsync(IChannel ch, CancellationToken ct = default)
    {
        await ch.ExchangeDeclareAsync(CatalogExchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
        await ch.ExchangeDeclareAsync(CatalogDlx, ExchangeType.Fanout, durable: true, cancellationToken: ct);

        // Orders queue with DLX
        await ch.QueueDeclareAsync(
            queue: OrdersQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = CatalogDlx },
            cancellationToken: ct);

        await ch.QueueBindAsync(
            queue: OrdersQueue,
            exchange: CatalogExchange,
            routingKey: ProductRoutingKeyPattern,
            cancellationToken: ct);

        // Orders DLQ
        await ch.QueueDeclareAsync(
            queue: OrdersDlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await ch.QueueBindAsync(
            queue: OrdersDlq,
            exchange: CatalogDlx,
            routingKey: "",
            cancellationToken: ct);

        // Reviews queue with DLX
        await ch.QueueDeclareAsync(
            queue: ReviewsQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = CatalogDlx },
            cancellationToken: ct);

        await ch.QueueBindAsync(
            queue: ReviewsQueue,
            exchange: CatalogExchange,
            routingKey: ProductRoutingKeyPattern,
            cancellationToken: ct);

        // Reviews DLQ
        await ch.QueueDeclareAsync(
            queue: ReviewsDlq,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await ch.QueueBindAsync(
            queue: ReviewsDlq,
            exchange: CatalogDlx,
            routingKey: "",
            cancellationToken: ct);
    }
}
