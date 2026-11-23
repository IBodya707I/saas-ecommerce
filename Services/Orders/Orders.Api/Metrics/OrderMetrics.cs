namespace Orders.Api.Metrics;

using System.Diagnostics.Metrics;

public class OrderMetrics
{
    private readonly Counter<long> _ordersCreated;
    private readonly Counter<long> _ordersConfirmed;
    private readonly Histogram<double> _operationDuration;
    private int _awaitingValidationCount = 0;

    public OrderMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("Orders.Api");

        _ordersCreated = meter.CreateCounter<long>(
            "orders.created",
            unit: "{order}",
            description: "Number of created orders");

        _ordersConfirmed = meter.CreateCounter<long>(
            "orders.confirmed",
            unit: "{order}",
            description: "Number of confirmed orders");

        _operationDuration = meter.CreateHistogram<double>(
            "orders.operation.duration",
            unit: "ms",
            description: "Duration of order operations");

        meter.CreateObservableGauge(
            "orders.awaiting_validation",
            () => _awaitingValidationCount,
            unit: "{order}",
            description: "Orders waiting for stock confirmation");
    }

    public void OrderCreated(string status) =>
        _ordersCreated.Add(1, new KeyValuePair<string, object?>("order.status", status));

    public void OrderConfirmed(string status) =>
        _ordersConfirmed.Add(1, new KeyValuePair<string, object?>("order.status", status));

    public void RecordDuration(string operation, bool success, double ms) =>
        _operationDuration.Record(ms,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("status", success ? "success" : "failure"));

    public void SetAwaitingValidationCount(int count) =>
        Interlocked.Exchange(ref _awaitingValidationCount, count);
}
