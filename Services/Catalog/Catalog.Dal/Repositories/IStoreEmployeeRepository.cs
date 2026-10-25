using Catalog.Domain.Entities;

namespace Catalog.Dal.Repositories;

public interface IStoreEmployeeRepository : IGenericRepository<StoreEmployee>
{
    Task<IReadOnlyList<StoreEmployee>> GetActiveStoreEmployeesWithUsersAsync(Guid storeId, CancellationToken ct = default);
    Task<IReadOnlyList<StoreEmployee>> GetStoresByUserIdAsync(Guid userId, CancellationToken ct = default);
}
