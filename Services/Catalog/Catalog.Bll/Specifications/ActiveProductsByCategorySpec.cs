using Ardalis.Specification;
using Catalog.Domain.Entities;

namespace Catalog.Bll.Specifications;

public class ActiveProductsByCategorySpec : Specification<Product>
{
    public ActiveProductsByCategorySpec(Guid categoryId)
    {
        Query.Where(p => p.CategoryId == categoryId 
                      && p.IsActive 
                      && !p.IsDeleted)
             .Include(p => p.Detail);
    }
}
