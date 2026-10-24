using System;

namespace Orders.Domain.Entities;

public class OrderStatusHistory
{
    public long Id { get; set; }
    public Guid OrderId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }
}
