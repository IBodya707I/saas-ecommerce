namespace Platform.Aggregator.HealthChecks;

using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

public class OrdersHealthCheck : IHealthCheck
{
    private readonly Health.HealthClient? _client;
    private readonly GrpcClientFactory? _clientFactory;
    private readonly ILogger<OrdersHealthCheck> _logger;

    public OrdersHealthCheck(Health.HealthClient client, ILogger<OrdersHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    public OrdersHealthCheck(GrpcClientFactory clientFactory, ILogger<OrdersHealthCheck> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var client = _client ?? _clientFactory?.CreateClient<Health.HealthClient>("OrdersHealth")
                ?? throw new InvalidOperationException("gRPC Health client is not configured");

            var reply = await client.CheckAsync(
                new HealthCheckRequest(),
                deadline: DateTime.UtcNow.AddSeconds(2),
                cancellationToken: ct);

            return reply.Status == HealthCheckResponse.Types.ServingStatus.Serving
                ? HealthCheckResult.Healthy("orders-api is serving")
                : HealthCheckResult.Unhealthy($"orders-api status: {reply.Status}");
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "orders-api health check failed");
            return HealthCheckResult.Unhealthy("orders-api is unavailable", ex);
        }
    }
}
