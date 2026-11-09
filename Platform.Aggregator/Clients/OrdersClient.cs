namespace Platform.Aggregator.Clients;

using System.Net.Http.Json;
using Platform.Aggregator.Models;

public class OrdersClient
{
    private readonly HttpClient _client;

    public OrdersClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<OrderDto?> GetOrderAsync(Guid id, Guid storeId, CancellationToken ct = default)
    {
        return await _client.GetFromJsonAsync<OrderDto>($"api/orders/{id}?storeId={storeId}", ct);
    }
}
