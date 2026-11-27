using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.Dal.Interfaces;

public interface IProcessedEventsRepository
{
    Task<bool> TryAddAsync(Guid eventId, CancellationToken ct = default);
}
