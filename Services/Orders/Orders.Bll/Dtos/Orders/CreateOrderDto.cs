using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object for placing an order with DataAnnotations validation.
/// Supports both registered users (CustomerId) and guest checkout (CustomerPhone, CustomerFullName).
/// </summary>
public class CreateOrderDto
{
    [Required]
    public Guid StoreId { get; set; }

    /// <summary>
    /// Optional Customer ID for authenticated users. If null, a guest customer is located or created by CustomerPhone.
    /// </summary>
    public Guid? CustomerId { get; set; }

    [Required(ErrorMessage = "Customer phone is required")]
    [RegularExpression(@"^\+[1-9][0-9]{7,14}$", ErrorMessage = "Customer phone must be in international format (e.g. +380501234567)")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer full name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Customer name must be between 2 and 150 characters")]
    public string CustomerFullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string? CustomerEmail { get; set; }

    [Required]
    [RegularExpression("^(NovaPoshta|UkrPoshta)$", ErrorMessage = "Delivery provider must be NovaPoshta or UkrPoshta")]
    public string DeliveryProvider { get; set; } = "NovaPoshta";

    [Required(ErrorMessage = "City is required")]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Warehouse address is required")]
    [StringLength(250, MinimumLength = 5)]
    public string WarehouseAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Recipient phone is required")]
    [RegularExpression(@"^\+[1-9][0-9]{7,14}$", ErrorMessage = "Recipient phone must be in international format (e.g. +380501234567)")]
    public string RecipientPhone { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [RegularExpression("^(Prepaid|CashOnDelivery)$", ErrorMessage = "Payment method must be Prepaid or CashOnDelivery")]
    public string PaymentMethod { get; set; } = "Prepaid";

    [Required(ErrorMessage = "Order must contain at least one item")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
