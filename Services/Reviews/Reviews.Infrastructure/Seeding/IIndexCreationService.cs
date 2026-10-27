namespace Reviews.Infrastructure.Seeding;

public interface IIndexCreationService
{
    Task CreateIndexesAsync(CancellationToken ct = default);
}
