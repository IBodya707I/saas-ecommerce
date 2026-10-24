using System;
using System.ComponentModel.DataAnnotations;

namespace Orders.Bll.Dtos.Orders;

/// <summary>
/// Data transfer object for updating the order status (State Machine transition).
/// </summary>
public class ChangeOrderStatusDto
{
    [Required]
    public Guid StoreId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ExpectedVersion must be a positive integer")]
    public int ExpectedVersion { get; set; }

    [Required]
    [RegularExpression("^(Submitted|Paid|Shipped|Cancelled)$", ErrorMessage = "Status must be Submitted, Paid, Shipped, or Cancelled")]
    public string NewStatus { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ChangedBy { get; set; } = "system";

    [StringLength(500)]
    public string? Comment { get; set; }
}
