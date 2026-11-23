namespace Catalog.Bll.Caching;

using System.Collections.Generic;
using System.Diagnostics.Metrics;

public class CacheMetrics
{
    private readonly Counter<long> _hits;
    private readonly Counter<long> _misses;

    public CacheMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Catalog.Cache");
        _hits = meter.CreateCounter<long>("cache.hits");
        _misses = meter.CreateCounter<long>("cache.misses");
    }

    public void Hit(string level) =>
        _hits.Add(1, new KeyValuePair<string, object?>("cache.level", level));

    public void Miss(string level) =>
        _misses.Add(1, new KeyValuePair<string, object?>("cache.level", level));
}
