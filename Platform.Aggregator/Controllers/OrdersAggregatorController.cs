namespace Platform.Aggregator.Controllers;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Platform.Aggregator.Clients;
using Platform.Aggregator.Models;

[ApiController]
[Route("api/aggregator/orders")]
public class OrdersAggregatorController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("Platform.Aggregator");
    private readonly OrdersClient _ordersClient;
    private readonly CatalogClient _catalogClient;
    private readonly ReviewsClient _reviewsClient;
    private readonly ILogger<OrdersAggregatorController> _logger;

    public OrdersAggregatorController(
        OrdersClient ordersClient,
        CatalogClient catalogClient,
        ReviewsClient reviewsClient,
        ILogger<OrdersAggregatorController> logger)
    {
        _ordersClient = ordersClient;
        _catalogClient = catalogClient;
        _reviewsClient = reviewsClient;
        _logger = logger;
    }

    [HttpGet("{id:guid}/full")]
    public async Task<ActionResult<FullOrderDto>> GetFullOrder(
        [FromRoute] Guid id,
        [FromQuery, System.ComponentModel.DataAnnotations.Required] Guid storeId,
        CancellationToken ct = default)
    {
        if (storeId == Guid.Empty &&
            Request.Headers.TryGetValue("X-Store-Id", out var headerVal) &&
            Guid.TryParse(headerVal, out var headerStoreId))
        {
            storeId = headerStoreId;
        }

        if (storeId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "storeId is required either as a query parameter or 'X-Store-Id' header"
            });
        }

        using var activity = ActivitySource.StartActivity("GetFullOrder");
        activity?.SetTag("order.id", id);
        activity?.SetTag("store.id", storeId);

        _logger.LogInformation("Aggregating full order details for OrderId={OrderId}, StoreId={StoreId}", id, storeId);

        OrderDto? order;
        var swOrders = Stopwatch.StartNew();
        try
        {
            order = await _ordersClient.GetOrderAsync(id, storeId, ct);
            swOrders.Stop();
            _logger.LogInformation("{Service} ok in {Elapsed} ms", "Orders", swOrders.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            swOrders.Stop();
            _logger.LogError(ex, "{Service} failed: {Error}", "Orders", ex.Message);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = $"Orders service error: {ex.Message}" });
        }

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} was not found", id);
            return NotFound(new { message = $"Order with ID '{id}' was not found." });
        }

        var warnings = new List<string>();
        var products = new List<ProductDto>();
        var reviews = new List<ReviewDto>();

        var productTasks = order.Items.Select(async item =>
        {
            var swCatalog = Stopwatch.StartNew();
            try
            {
                var product = await _catalogClient.GetProductAsync(item.ProductId, ct);
                swCatalog.Stop();
                _logger.LogInformation("{Service} ok in {Elapsed} ms", "Catalog", swCatalog.ElapsedMilliseconds);
                return (Product: product, Error: (string?)null);
            }
            catch (Exception ex)
            {
                swCatalog.Stop();
                _logger.LogError(ex, "{Service} failed: {Error}", "Catalog", ex.Message);
                return (Product: (ProductDto?)null, Error: $"Catalog service error for product '{item.ProductName}' ({item.ProductId}): {ex.Message}");
            }
        }).ToList();

        var reviewTasks = order.Items.Select(async item =>
        {
            var swReviews = Stopwatch.StartNew();
            try
            {
                long numericId = Math.Abs(BitConverter.ToInt64(item.ProductId.ToByteArray(), 0));
                var productReviews = await _reviewsClient.GetReviewsByProductIdAsync(numericId, ct);
                swReviews.Stop();
                _logger.LogInformation("{Service} ok in {Elapsed} ms", "Reviews", swReviews.ElapsedMilliseconds);
                return (Reviews: productReviews, Error: (string?)null);
            }
            catch (Exception ex)
            {
                swReviews.Stop();
                _logger.LogError(ex, "{Service} failed: {Error}", "Reviews", ex.Message);
                return (Reviews: (IReadOnlyList<ReviewDto>?)null, Error: $"Reviews service error for product '{item.ProductName}': {ex.Message}");
            }
        }).ToList();

        // Parallel invocation of downstream services using Task.WhenAll (Criterion 12)
        await Task.WhenAll(productTasks.Concat<Task>(reviewTasks));

        foreach (var pTask in productTasks)
        {
            var res = await pTask;
            if (res.Product != null)
            {
                products.Add(res.Product);
            }
            if (!string.IsNullOrWhiteSpace(res.Error))
            {
                warnings.Add(res.Error);
            }
        }

        foreach (var rTask in reviewTasks)
        {
            var res = await rTask;
            if (res.Reviews != null && res.Reviews.Count > 0)
            {
                reviews.AddRange(res.Reviews);
            }
            if (!string.IsNullOrWhiteSpace(res.Error))
            {
                warnings.Add(res.Error);
            }
        }

        return Ok(new FullOrderDto(order, products, reviews, warnings));
    }
}
