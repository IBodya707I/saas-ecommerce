namespace Platform.Aggregator.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Api.Grpc;
using global::Grpc.Core;
using GrpcStatusCode = global::Grpc.Core.StatusCode;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Orders.Api.Grpc;
using Platform.Aggregator.Models;
using Reviews.WebApi.Grpc;

[ApiController]
[Route("api/aggregator/orders")]
public class OrdersAggregatorController : ControllerBase
{
    private static readonly ActivitySource ActivitySource = new("Platform.Aggregator");
    private readonly OrdersService.OrdersServiceClient _ordersClient;
    private readonly CatalogService.CatalogServiceClient _catalogClient;
    private readonly ReviewsService.ReviewsServiceClient _reviewsClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<OrdersAggregatorController> _logger;

    public OrdersAggregatorController(
        OrdersService.OrdersServiceClient ordersClient,
        CatalogService.CatalogServiceClient catalogClient,
        ReviewsService.ReviewsServiceClient reviewsClient,
        IDistributedCache cache,
        ILogger<OrdersAggregatorController> logger)
    {
        _ordersClient = ordersClient;
        _catalogClient = catalogClient;
        _reviewsClient = reviewsClient;
        _cache = cache;
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

        var cacheKey = $"aggregator:order:{id}:{storeId}:full";
        var cachedBytes = await _cache.GetAsync(cacheKey, ct);
        if (cachedBytes is not null)
        {
            _logger.LogInformation("Cache hit for aggregated order {CacheKey}", cacheKey);
            var cachedResult = JsonSerializer.Deserialize<FullOrderDto>(cachedBytes);
            if (cachedResult is not null)
            {
                return Ok(cachedResult);
            }
        }

        _logger.LogInformation("Cache miss for aggregated order {CacheKey}", cacheKey);

        OrderDto? order;
        var swOrders = Stopwatch.StartNew();
        try
        {
            var orderReply = await _ordersClient.GetOrderAsync(
                new GetOrderRequest { Id = id.ToString(), StoreId = storeId.ToString() },
                cancellationToken: ct);

            swOrders.Stop();
            _logger.LogInformation("{Service} ok in {Elapsed} ms", "Orders", swOrders.ElapsedMilliseconds);
            order = MapToOrderDto(orderReply);
        }
        catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.NotFound)
        {
            swOrders.Stop();
            _logger.LogWarning("Order {OrderId} was not found (404 from Orders gRPC service)", id);
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = $"Order with ID '{id}' was not found."
            });
        }
        catch (RpcException ex)
        {
            swOrders.Stop();
            _logger.LogError(ex, "{Service} failed: {Error}", "Orders", ex.Status.Detail);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = $"Orders service gRPC error: {ex.Status.Detail}" });
        }
        catch (Exception ex)
        {
            swOrders.Stop();
            _logger.LogError(ex, "{Service} failed: {Error}", "Orders", ex.Message);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = $"Orders service error: {ex.Message}" });
        }

        var warnings = new List<string>();
        var products = new List<ProductDto>();
        var reviews = new List<ReviewDto>();

        var productTasks = order.Items.Select(async item =>
        {
            var swCatalog = Stopwatch.StartNew();
            try
            {
                var pReply = await _catalogClient.GetProductAsync(
                    new GetProductRequest { Id = item.ProductId.ToString() },
                    cancellationToken: ct);

                swCatalog.Stop();
                _logger.LogInformation("{Service} ok in {Elapsed} ms", "Catalog", swCatalog.ElapsedMilliseconds);
                return (Product: MapToProductDto(pReply), Error: (string?)null);
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.Unavailable || ex.StatusCode == GrpcStatusCode.DeadlineExceeded)
            {
                swCatalog.Stop();
                _logger.LogWarning(ex, "gRPC Catalog unavailable: {Status}", ex.Status);
                return (Product: (ProductDto?)null, Error: $"Catalog service unavailable for product '{item.ProductName}': {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                swCatalog.Stop();
                _logger.LogError(ex, "{Service} failed: {Error}", "Catalog", ex.Message);
                return (Product: (ProductDto?)null, Error: $"Catalog service error for product '{item.ProductName}': {ex.Message}");
            }
        }).ToList();

        var reviewTasks = order.Items.Select(async item =>
        {
            var swReviews = Stopwatch.StartNew();
            try
            {
                long numericId = Math.Abs(BitConverter.ToInt64(item.ProductId.ToByteArray(), 0));
                var rReply = await _reviewsClient.GetReviewsByProductIdAsync(
                    new GetReviewsByProductIdRequest { ProductId = numericId, Page = 1, PageSize = 20 },
                    cancellationToken: ct);

                swReviews.Stop();
                _logger.LogInformation("{Service} ok in {Elapsed} ms", "Reviews", swReviews.ElapsedMilliseconds);
                var dtos = rReply.Items.Select(MapToReviewDto).ToList();
                return (Reviews: (IReadOnlyList<ReviewDto>)dtos, Error: (string?)null);
            }
            catch (RpcException ex) when (ex.StatusCode == GrpcStatusCode.Unavailable || ex.StatusCode == GrpcStatusCode.DeadlineExceeded)
            {
                swReviews.Stop();
                _logger.LogWarning(ex, "gRPC Reviews unavailable: {Status}", ex.Status);
                return (Reviews: (IReadOnlyList<ReviewDto>?)null, Error: $"Reviews service unavailable for product '{item.ProductName}': {ex.Status.Detail}");
            }
            catch (Exception ex)
            {
                swReviews.Stop();
                _logger.LogError(ex, "{Service} failed: {Error}", "Reviews", ex.Message);
                return (Reviews: (IReadOnlyList<ReviewDto>?)null, Error: $"Reviews service error for product '{item.ProductName}': {ex.Message}");
            }
        }).ToList();

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

        var fullOrder = new FullOrderDto(order, products, reviews, warnings);

        if (warnings.Count == 0)
        {
            var serialized = JsonSerializer.SerializeToUtf8Bytes(fullOrder);
            await _cache.SetAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(45)
            }, ct);
            _logger.LogInformation("Cached aggregated order in Redis for key {CacheKey}", cacheKey);
        }

        return Ok(fullOrder);
    }

    private static OrderDto MapToOrderDto(OrderReply o)
    {
        return new OrderDto(
            Guid.Parse(o.Id),
            Guid.Parse(o.StoreId),
            Guid.Parse(o.CustomerId),
            o.Status,
            o.PaymentMethod,
            (decimal)o.TotalAmount,
            o.Version,
            DateTime.TryParse(o.CreatedAt, out var dt) ? dt : DateTime.UtcNow,
            o.Details != null
                ? new OrderDetailsDto(
                    o.Details.WarehouseAddress,
                    o.Details.City,
                    "00000",
                    o.Details.DeliveryProvider,
                    null,
                    o.Details.RecipientPhone)
                : null,
            o.Items.Select(i => new OrderItemDto(
                Guid.Parse(i.ProductId),
                i.ProductName,
                (decimal)i.UnitPrice,
                i.Quantity)).ToList()
        );
    }

    private static ProductDto MapToProductDto(ProductReply p)
    {
        return new ProductDto(
            Guid.Parse(p.Id),
            string.IsNullOrEmpty(p.StoreId) ? Guid.Empty : Guid.Parse(p.StoreId),
            string.IsNullOrEmpty(p.CategoryId) ? Guid.Empty : Guid.Parse(p.CategoryId),
            p.Name,
            (decimal)p.Price,
            p.Sku,
            p.AvailableStock,
            p.IsActive
        );
    }

    private static ReviewDto MapToReviewDto(ReviewReply r)
    {
        return new ReviewDto(
            r.Id,
            r.ProductId,
            r.ProductName,
            r.AuthorName,
            r.Rating,
            r.Text,
            r.CommentsCount,
            r.Version,
            DateTime.TryParse(r.CreatedAt, out var dt) ? dt : DateTime.UtcNow
        );
    }
}
