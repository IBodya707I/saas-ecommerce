namespace Platform.Aggregator.Clients;

using System.Net.Http.Json;
using Platform.Aggregator.Models;

public class ReviewsClient
{
    private readonly HttpClient _client;

    public ReviewsClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsByProductIdAsync(long productId, CancellationToken ct = default)
    {
        var result = await _client.GetFromJsonAsync<ReviewsPagedResult>($"api/reviews?productId={productId}", ct);
        return result?.Items ?? Array.Empty<ReviewDto>();
    }

    public async Task<IReadOnlyList<ReviewDto>> GetReviewsAsync(CancellationToken ct = default)
    {
        var result = await _client.GetFromJsonAsync<ReviewsPagedResult>("api/reviews", ct);
        return result?.Items ?? Array.Empty<ReviewDto>();
    }
}
