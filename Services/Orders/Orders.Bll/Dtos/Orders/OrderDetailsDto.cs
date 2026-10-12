namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object representing delivery details in API responses.
/// </summary>
public record OrderDetailsDto(
    string DeliveryProvider,
    string City,
    string WarehouseAddress,
    string RecipientPhone,
    string? Notes);
