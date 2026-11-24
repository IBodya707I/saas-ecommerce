using System;

namespace Platform.EventBus.Events;

public record ProductRenamedEvent(
    Guid ProductId,
    string OldName,
    string NewName) : IntegrationEvent
{
    public string ProductName => NewName;
}
