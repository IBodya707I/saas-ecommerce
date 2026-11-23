namespace Platform.Aggregator.HealthChecks;

using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

public class CatalogHealthCheck : IHealthCheck
{
    private readonly GrpcClientFactory _clientFactory;
    private readonly ILogger<CatalogHealthCheck> _logger;

    public CatalogHealthCheck(GrpcClientFactory clientFactory, ILogger<CatalogHealthCheck> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        try
        {
            var client = _clientFactory.CreateClient<Health.HealthClient>("CatalogHealth");

            var reply = await client.CheckAsync(
                new HealthCheckRequest(),
                deadline: DateTime.UtcNow.AddSeconds(2),
                cancellationToken: ct);

            return reply.Status == HealthCheckResponse.Types.ServingStatus.Serving
                ? HealthCheckResult.Healthy("catalog-api is serving")
                : HealthCheckResult.Unhealthy($"catalog-api status: {reply.Status}");
        }
        catch (RpcException ex)
        {
            _logger.LogWarning(ex, "catalog-api health check failed");
            return HealthCheckResult.Unhealthy("catalog-api is unavailable", ex);
        }
    }
}
