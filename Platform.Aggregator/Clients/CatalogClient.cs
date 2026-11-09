namespace Platform.Aggregator.Clients;

using System.Net.Http.Json;
using Platform.Aggregator.Models;

public class CatalogClient
{
    private readonly HttpClient _client;

    public CatalogClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<ProductDto?> GetProductAsync(Guid id, CancellationToken ct = default)
    {
        return await _client.GetFromJsonAsync<ProductDto>($"api/products/{id}", ct);
    }
}
