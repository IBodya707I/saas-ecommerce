using System;

namespace Orders.Domain.Entities;

public class OrderItem
{
    public Guid OrderId { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; 
    public decimal UnitPrice { get; set; }                  
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
