using Catalog.Domain.Entities;
using Catalog.Dal.Repositories;

namespace Catalog.Dal;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IStoreEmployeeRepository StoreEmployees { get; }
    IGenericRepository<Store> Stores { get; }
    IGenericRepository<User> Users { get; }

    void SetStoreContext(Guid? storeId);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
