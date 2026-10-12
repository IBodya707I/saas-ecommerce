using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Orders.Bll.Dtos.Orders;
using Orders.Bll.Interfaces;
using Orders.Dal.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Bll.Services;

/// <summary>
/// Implementation of the order service business logic.
/// Encapsulates multi-repository transactions, domain invariants, and stored procedure coordination.
/// </summary>
public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OrderDto> GetByIdAsync(Guid orderId, Guid storeId, CancellationToken ct = default)
    {
        var order = await _uow.Orders.GetWithItemsAsync(orderId, storeId, ct)
            ?? throw new NotFoundException($"Order {orderId} was not found");

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct = default)
    {
        var orders = await _uow.Orders.GetByCustomerAsync(customerId, ct);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByStoreAndStatusAsync(Guid storeId, string status, CancellationToken ct = default)
    {
        var orders = await _uow.Orders.GetByStoreAndStatusAsync(storeId, status, ct);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        if (dto.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one product item");
        }

        Guid customerId;
        string? effectiveNotes = dto.Notes;

        if (dto.CustomerId.HasValue && dto.CustomerId.Value != Guid.Empty)
        {
            var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId.Value, dto.StoreId, ct)
                ?? throw new NotFoundException($"Customer {dto.CustomerId.Value} was not found in store {dto.StoreId}");
            customerId = customer.Id;
        }
        else
        {
            var existingCustomer = await _uow.Customers.GetByPhoneAsync(dto.CustomerPhone, dto.StoreId, ct);
            if (existingCustomer is not null)
            {
                customerId = existingCustomer.Id;

                if (!existingCustomer.IsRegistered)
                {
                    // Guest customer: update name/email if updated details were provided
                    bool nameChanged = !string.Equals(existingCustomer.FullName, dto.CustomerFullName, StringComparison.OrdinalIgnoreCase);
                    bool emailChanged = !string.IsNullOrWhiteSpace(dto.CustomerEmail) &&
                                        !string.Equals(existingCustomer.Email, dto.CustomerEmail, StringComparison.OrdinalIgnoreCase);

                    if (nameChanged || emailChanged)
                    {
                        existingCustomer.FullName = dto.CustomerFullName;
                        if (!string.IsNullOrWhiteSpace(dto.CustomerEmail))
                        {
                            existingCustomer.Email = dto.CustomerEmail;
                        }

                        await _uow.Customers.UpdateAsync(existingCustomer, existingCustomer.Version, ct);
                    }
                }
                else
                {
                    // Registered customer profile is protected; if a different recipient name was specified, note it
                    if (!string.Equals(existingCustomer.FullName, dto.CustomerFullName, StringComparison.OrdinalIgnoreCase))
                    {
                        effectiveNotes = string.IsNullOrWhiteSpace(dto.Notes)
                            ? $"Recipient name: {dto.CustomerFullName}"
                            : $"Recipient name: {dto.CustomerFullName}. {dto.Notes}";
                    }
                }
            }
            else
            {
                // Create a new guest customer record (is_registered = false, password_hash = null)
                var guestCustomer = new Customer
                {
                    Id = Guid.NewGuid(),
                    StoreId = dto.StoreId,
                    Phone = dto.CustomerPhone,
                    FullName = dto.CustomerFullName,
                    Email = dto.CustomerEmail,
                    IsRegistered = false,
                    PasswordHash = null
                };

                customerId = await _uow.Customers.AddAsync(guestCustomer, ct);
            }
        }

        var orderId = Guid.NewGuid();
        var items = dto.Items.Select(x => new OrderItem
        {
            OrderId = orderId,
            StoreId = dto.StoreId,
            ProductId = x.ProductId,
            Quantity = x.Quantity
        }).ToList();

        // Invokes usp_create_order stored procedure with stock validation, reservation, and details insertion
        await _uow.Orders.CreateOrderProcedureAsync(
            orderId,
            customerId,
            dto.StoreId,
            dto.DeliveryProvider,
            dto.City,
            dto.WarehouseAddress,
            dto.RecipientPhone,
            effectiveNotes,
            dto.PaymentMethod,
            items,
            ct);

        var created = await _uow.Orders.GetWithItemsAsync(orderId, dto.StoreId, ct)
            ?? throw new NotFoundException($"Order {orderId} could not be retrieved after placement");

        return _mapper.Map<OrderDto>(created);
    }

    /// <summary>
    /// Transactional business operation spanning multiple repositories (Orders + Products)
    /// satisfying Lab 02 criteria B.4 and C.2.
    /// Handles payment confirmation taking into account Prepaid vs CashOnDelivery state machine rules.
    /// </summary>
    public async Task PayOrderAsync(Guid orderId, Guid storeId, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        try
        {
            var order = await _uow.Orders.GetWithItemsAsync(orderId, storeId, ct)
                ?? throw new NotFoundException($"Order {orderId} not found");

            if (order.Items.Count == 0)
            {
                throw new BusinessConflictException($"Order {orderId} has no items to pay for");
            }

            // State Machine payment rules matching db/p1/procedures.sql:
            // 1. Prepaid: moves from 'Submitted' -> 'Paid'
            // 2. CashOnDelivery: moves from 'Shipped' -> 'Paid' (payment upon courier delivery)
            if (order.PaymentMethod == "Prepaid")
            {
                if (order.Status != "Submitted")
                {
                    throw new BusinessConflictException($"Cannot pay Prepaid order {orderId} with status '{order.Status}'. Expected 'Submitted'.");
                }
            }
            else if (order.PaymentMethod == "CashOnDelivery")
            {
                if (order.Status != "Shipped")
                {
                    throw new BusinessConflictException($"Cannot record payment for CashOnDelivery order {orderId} with status '{order.Status}'. Expected 'Shipped'.");
                }
            }

            // 1. Update order status to 'Paid' (invokes usp_change_order_status)
            await _uow.Orders.ChangeStatusAsync(
                orderId,
                storeId,
                order.Version,
                "Paid",
                "OrderService.PayOrder",
                $"Payment confirmed for {order.PaymentMethod} order",
                ct);

            // 2. Cross-repository interaction with Products repository within the same transaction
            foreach (var item in order.Items)
            {
                await _uow.Products.DecreaseStockAsync(storeId, item.ProductId, 0, ct);
            }

            await _uow.CommitAsync(ct);
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ChangeStatusAsync(Guid orderId, ChangeOrderStatusDto dto, CancellationToken ct = default)
    {
        await _uow.Orders.ChangeStatusAsync(
            orderId,
            dto.StoreId,
            dto.ExpectedVersion,
            dto.NewStatus,
            dto.ChangedBy,
            dto.Comment,
            ct);
    }
}
