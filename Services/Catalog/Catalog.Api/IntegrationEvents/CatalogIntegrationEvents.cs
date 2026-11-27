using System;
using Platform.EventBus.Events;

namespace Catalog.Api.IntegrationEvents;

public record ProductPriceChangedIntegrationEvent(
    Guid ProductId,
    string ProductName,
    decimal OldPrice,
    decimal NewPrice) : ProductPriceChangedEvent(ProductId, ProductName, OldPrice, NewPrice);

public record ProductRenamedIntegrationEvent(
    Guid ProductId,
    string OldName,
    string NewName) : ProductRenamedEvent(ProductId, OldName, NewName);
