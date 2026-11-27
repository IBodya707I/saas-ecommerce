using System.Threading;
using System.Threading.Tasks;
using Platform.EventBus.Events;

namespace Platform.EventBus.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string exchange, string routingKey, CancellationToken ct = default)
        where T : IntegrationEvent;
}
