using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Bll.Dtos.Customers;

namespace Orders.Bll.Interfaces;

/// <summary>
/// Service contract for customer management business logic.
/// </summary>
public interface ICustomerService
{
    Task<CustomerDto?> GetByIdAsync(Guid id, Guid storeId, CancellationToken ct = default);
    Task<CustomerDto?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerDto>> GetAllByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid storeId, CancellationToken ct = default);
}
