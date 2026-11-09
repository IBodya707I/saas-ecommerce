namespace Catalog.Bll.Caching;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ITwoLevelCacheService
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T?>> factory,
        TimeSpan? l1Expiration = null,
        TimeSpan? l2Expiration = null,
        CancellationToken ct = default);

    Task RemoveAsync(string key, string reason, CancellationToken ct = default);
}
