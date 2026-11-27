using System;
using System.Threading;
using System.Threading.Tasks;

namespace Reviews.Infrastructure.Persistence;

public interface IProcessedEventsStore
{
    Task<bool> TryAddAsync(Guid eventId, CancellationToken ct = default);
}
