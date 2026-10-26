using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal;

public class CatalogDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public Guid? CurrentStoreId => _tenantContext?.CurrentStoreId;

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        ITenantContext? tenantContext = null) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductDetail> ProductDetails => Set<ProductDetail>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<StoreEmployee> StoreEmployees => Set<StoreEmployee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => CurrentStoreId == null || p.StoreId == CurrentStoreId);

        modelBuilder.Entity<ProductDetail>()
            .HasQueryFilter(pd => CurrentStoreId == null || pd.Product!.StoreId == CurrentStoreId);

        modelBuilder.Entity<ProductImage>()
            .HasQueryFilter(pi => CurrentStoreId == null || pi.Product!.StoreId == CurrentStoreId);

        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => CurrentStoreId == null || c.StoreId == CurrentStoreId);

        modelBuilder.Entity<StoreEmployee>()
            .HasQueryFilter(se => CurrentStoreId == null || se.StoreId == CurrentStoreId);
    }
}
