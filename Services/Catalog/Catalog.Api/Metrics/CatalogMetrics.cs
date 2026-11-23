namespace Catalog.Api.Metrics;

using System.Diagnostics.Metrics;

public class CatalogMetrics
{
    private readonly Counter<long> _productsUpdated;
    private readonly Histogram<double> _operationDuration;
    private int _activeProductsCount = 0;

    public CatalogMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Catalog.Api");

        _productsUpdated = meter.CreateCounter<long>(
            "products.updated",
            unit: "{product}",
            description: "Number of updated products");

        _operationDuration = meter.CreateHistogram<double>(
            "products.operation.duration",
            unit: "ms",
            description: "Duration of product operations");

        meter.CreateObservableGauge(
            "products.active_count",
            () => _activeProductsCount,
            unit: "{product}",
            description: "Active products count");
    }

    public void ProductUpdated(string operation) =>
        _productsUpdated.Add(1, new KeyValuePair<string, object?>("operation", operation));

    public void RecordDuration(string operation, bool success, double ms) =>
        _operationDuration.Record(ms,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("status", success ? "success" : "failure"));

    public void SetActiveProductsCount(int count) =>
        Interlocked.Exchange(ref _activeProductsCount, count);
}
