using Catalog.Domain.Entities;

namespace Catalog.Dal.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetWithSubCategoriesExplicitAsync(Guid id, CancellationToken ct = default);
}
