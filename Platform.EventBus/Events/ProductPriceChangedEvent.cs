using System;

namespace Platform.EventBus.Events;

public record ProductPriceChangedEvent(
    Guid ProductId,
    string ProductName,
    decimal OldPrice,
    decimal NewPrice) : IntegrationEvent;
