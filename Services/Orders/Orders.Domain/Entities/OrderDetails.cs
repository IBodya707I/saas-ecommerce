using System;

namespace Orders.Domain.Entities;

public class OrderDetails
{
    public Guid OrderId { get; set; }
    public string DeliveryProvider { get; set; } = "NovaPoshta";
    public string City { get; set; } = string.Empty;
    public string WarehouseAddress { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
