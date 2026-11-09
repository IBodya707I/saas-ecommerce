namespace Catalog.Bll.Caching;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class TwoLevelCacheService : ITwoLevelCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly CacheMetrics _metrics;
    private readonly ILogger<TwoLevelCacheService> _logger;

    public TwoLevelCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        CacheMetrics metrics,
        ILogger<TwoLevelCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T?>> factory,
        TimeSpan? l1Expiration = null,
        TimeSpan? l2Expiration = null,
        CancellationToken ct = default)
    {
        if (_memoryCache.TryGetValue(key, out T? l1Value))
        {
            _metrics.Hit("L1");
            _logger.LogInformation("Cache hit L1 for key {Key}", key);
            return l1Value;
        }

        _metrics.Miss("L1");
        _logger.LogInformation("Cache miss L1 for key {Key}", key);

        byte[]? l2Bytes = null;
        try
        {
            l2Bytes = await _distributedCache.GetAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read from Redis L2 cache for key {Key}", key);
        }

        if (l2Bytes is not null)
        {
            _metrics.Hit("L2");
            _logger.LogInformation("Cache hit L2 for key {Key}", key);

            var deserialized = JsonSerializer.Deserialize<T>(l2Bytes);
            if (deserialized is not null)
            {
                SetL1(key, deserialized, l1Expiration);
                return deserialized;
            }
        }

        _metrics.Miss("L2");
        _logger.LogInformation("Cache miss L2 for key {Key}", key);

        var dbValue = await factory();
        if (dbValue is null)
        {
            return default;
        }

        try
        {
            var bytesToCache = JsonSerializer.SerializeToUtf8Bytes(dbValue);
            var l2Options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = l2Expiration ?? TimeSpan.FromMinutes(10)
            };
            await _distributedCache.SetAsync(key, bytesToCache, l2Options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write to Redis L2 cache for key {Key}", key);
        }

        SetL1(key, dbValue, l1Expiration);
        return dbValue;
    }

    public async Task RemoveAsync(string key, string reason, CancellationToken ct = default)
    {
        _memoryCache.Remove(key);
        try
        {
            await _distributedCache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove key {Key} from Redis L2 cache", key);
        }

        _logger.LogInformation("Cache invalidated {Key}, reason: {Reason}", key, reason);
    }

    private void SetL1<T>(string key, T value, TimeSpan? expiration)
    {
        var options = new MemoryCacheEntryOptions
        {
            Size = 1,
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(1)
        };
        _memoryCache.Set(key, value, options);
    }
}
