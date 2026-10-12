using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orders.Domain.Entities;

namespace Orders.Dal.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, Guid storeId, CancellationToken ct = default);
    Task<Customer?> GetByPhoneAsync(string phone, Guid storeId, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllByStoreAsync(Guid storeId, CancellationToken ct = default);
    Task<Guid> AddAsync(Customer customer, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, int expectedVersion, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid storeId, CancellationToken ct = default);
}
