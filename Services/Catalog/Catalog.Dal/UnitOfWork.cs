using Catalog.Domain.Entities;
using Catalog.Dal.Repositories;

namespace Catalog.Dal;

public class UnitOfWork : IUnitOfWork
{
    private readonly CatalogDbContext _context;
    private readonly ITenantContext? _tenantContext;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IStoreEmployeeRepository? _storeEmployees;
    private IGenericRepository<Store>? _stores;
    private IGenericRepository<User>? _users;
    private bool _disposed;

    public UnitOfWork(CatalogDbContext context, ITenantContext? tenantContext = null)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public IStoreEmployeeRepository StoreEmployees =>
        _storeEmployees ??= new StoreEmployeeRepository(_context);

    public IGenericRepository<Store> Stores =>
        _stores ??= new GenericRepository<Store>(_context);

    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    public void SetStoreContext(Guid? storeId)
    {
        if (_tenantContext != null)
        {
            _tenantContext.CurrentStoreId = storeId;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
