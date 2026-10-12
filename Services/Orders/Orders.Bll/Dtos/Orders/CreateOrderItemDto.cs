using System;
using System.ComponentModel.DataAnnotations;

namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object for a cart item line when creating an order.
/// </summary>
public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }
}
