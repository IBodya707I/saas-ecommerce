using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orders.Bll.Dtos.Customers;
using Orders.Bll.Interfaces;

namespace Orders.Api.Controllers;

/// <summary>
/// Controller for managing store customers.
/// </summary>
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(
        [FromRoute] Guid id,
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        var customer = await _customerService.GetByIdAsync(id, storeId, ct);
        if (customer is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with ID '{id}' was not found in store '{storeId}'.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(customer);
    }

    /// <summary>
    /// Retrieves a customer by phone number within a store.
    /// </summary>
    [HttpGet("by-phone")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetByPhone(
        [FromQuery] string phone,
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        var customer = await _customerService.GetByPhoneAsync(phone, storeId, ct);
        if (customer is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Customer Not Found",
                Detail = $"Customer with phone '{phone}' was not found in store '{storeId}'.",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(customer);
    }

    /// <summary>
    /// Retrieves all active customers for a specific store.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        var customers = await _customerService.GetAllByStoreAsync(storeId, ct);
        return Ok(customers);
    }

    /// <summary>
    /// Creates or registers a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerDto dto,
        CancellationToken ct)
    {
        var created = await _customerService.CreateAsync(dto, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id, storeId = created.StoreId },
            created);
    }

    /// <summary>
    /// Performs a soft deletion of a customer.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        await _customerService.DeleteAsync(id, storeId, ct);
        return NoContent();
    }
}
