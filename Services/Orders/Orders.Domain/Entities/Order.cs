using System;
using System.Collections.Generic;

namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StoreId { get; set; }
    public string Status { get; set; } = "Submitted";
    public string PaymentMethod { get; set; } = "Prepaid";
    public decimal TotalAmount { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties (Order aggregate)
    public List<OrderItem> Items { get; set; } = new();
    public OrderDetails? Details { get; set; }
}
