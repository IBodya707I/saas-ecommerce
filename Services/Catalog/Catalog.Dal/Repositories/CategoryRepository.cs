using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(CatalogDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetWithSubCategoriesExplicitAsync(Guid id, CancellationToken ct = default)
    {
        var category = await Set.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        if (category == null)
            return null;

        await Context.Entry(category)
            .Collection(c => c.SubCategories)
            .Query()
            .Where(sc => !sc.IsDeleted)
            .LoadAsync(ct);

        return category;
    }
}
