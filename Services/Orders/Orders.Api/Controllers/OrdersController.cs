using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orders.Bll.Dtos.Orders;
using Orders.Bll.Interfaces;

namespace Orders.Api.Controllers;

/// <summary>
/// Controller for managing orders, checkout, and status workflows.
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Retrieves a complete order aggregate with items and delivery details.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(
        [FromRoute] Guid id,
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(id, storeId, ct);
        return Ok(order);
    }

    /// <summary>
    /// Retrieves order history for a customer.
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByCustomer(
        [FromRoute] Guid customerId,
        CancellationToken ct)
    {
        var orders = await _orderService.GetByCustomerAsync(customerId, ct);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves orders filtered by store and status for merchant dashboard.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetByStoreAndStatus(
        [FromQuery] Guid storeId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var statusFilter = string.IsNullOrWhiteSpace(status) ? "Submitted" : status;
        var orders = await _orderService.GetByStoreAndStatusAsync(storeId, statusFilter, ct);
        return Ok(orders);
    }

    /// <summary>
    /// Creates and places a new order. Supports registered customers and guest checkout.
    /// Coordinates stock deduction, price snapshotting, and logistic details insertion.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderDto dto,
        CancellationToken ct)
    {
        var created = await _orderService.CreateAsync(dto, ct);
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id, storeId = created.StoreId },
            created);
    }

    /// <summary>
    /// Confirms and records order payment in a multi-repository transaction.
    /// Validates payment method transitions (Prepaid vs CashOnDelivery).
    /// </summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PayOrder(
        [FromRoute] Guid id,
        [FromQuery] Guid storeId,
        CancellationToken ct)
    {
        await _orderService.PayOrderAsync(id, storeId, ct);
        return Ok(new { message = $"Order '{id}' payment processed successfully." });
    }

    /// <summary>
    /// Updates the order status enforcing optimistic concurrency and State Machine transitions.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeOrderStatusDto dto,
        CancellationToken ct)
    {
        await _orderService.ChangeStatusAsync(id, dto, ct);
        return Ok(new { message = $"Order '{id}' status transitioned to '{dto.NewStatus}'." });
    }
}
