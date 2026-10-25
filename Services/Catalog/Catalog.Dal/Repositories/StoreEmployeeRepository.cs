using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public class StoreEmployeeRepository : GenericRepository<StoreEmployee>, IStoreEmployeeRepository
{
    public StoreEmployeeRepository(CatalogDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<StoreEmployee>> GetActiveStoreEmployeesWithUsersAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        return await Set
            .Include(se => se.User)
            .Where(se => se.StoreId == storeId && se.IsActive && !se.IsDeleted)
            .OrderByDescending(se => se.AssignedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StoreEmployee>> GetStoresByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await Set
            .Include(se => se.Store)
            .Where(se => se.UserId == userId && se.IsActive && !se.IsDeleted)
            .OrderByDescending(se => se.AssignedAt)
            .ToListAsync(ct);
    }
}
