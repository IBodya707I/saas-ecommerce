using System;
using System.ComponentModel.DataAnnotations;

namespace Orders.Bll.Dtos.Customers;

/// <summary>
/// Data transfer object for creating a new customer with DataAnnotations validation.
/// </summary>
public class CreateCustomerDto
{
    [Required]
    public Guid StoreId { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\+[1-9][0-9]{7,14}$", ErrorMessage = "Phone must be in international format (e.g. +380501234567)")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
}
