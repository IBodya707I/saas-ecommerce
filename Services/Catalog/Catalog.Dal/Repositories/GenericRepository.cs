using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly CatalogDbContext Context;
    protected readonly DbSet<T> Set;

    public GenericRepository(CatalogDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Set.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await Set.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
    }

    public virtual void Update(T entity)
    {
        Set.Update(entity);
    }

    public virtual void Delete(T entity)
    {
        Set.Remove(entity);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = ApplySpecification(spec);
        return await query.ToListAsync(ct);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = ApplySpecification(spec);
        return await query.FirstOrDefaultAsync(ct);
    }

    public virtual async Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = ApplySpecification(spec, evaluateCriteriaOnly: true);
        return await query.CountAsync(ct);
    }

    protected IQueryable<T> ApplySpecification(ISpecification<T> spec, bool evaluateCriteriaOnly = false)
    {
        return evaluateCriteriaOnly
            ? SpecificationEvaluator.Default.GetQuery(Set.AsQueryable(), spec, evaluateCriteriaOnly: true)
            : Set.WithSpecification(spec);
    }
}
