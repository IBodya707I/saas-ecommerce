namespace Catalog.Dal;

public interface ITenantContext
{
    Guid? CurrentStoreId { get; set; }
}

public class TenantContext : ITenantContext
{
    public Guid? CurrentStoreId { get; set; }
}
