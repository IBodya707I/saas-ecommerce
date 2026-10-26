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
        string? role = null,
        DateTime? assignedSince = null,
        CancellationToken ct = default)
    {
        var query = Set
            .Include(se => se.User)
            .Where(se => se.IsActive && !se.IsDeleted);

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(se => se.Role == role);

        if (assignedSince.HasValue)
            query = query.Where(se => se.AssignedAt >= assignedSince.Value);

        return await query
            .OrderByDescending(se => se.AssignedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StoreEmployee>> GetStoresByUserIdAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await Set
            .IgnoreQueryFilters()
            .Include(se => se.Store)
            .Where(se => se.UserId == userId && se.IsActive && !se.IsDeleted)
            .OrderByDescending(se => se.AssignedAt)
            .ToListAsync(ct);
    }
}
