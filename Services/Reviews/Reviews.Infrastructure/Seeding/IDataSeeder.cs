namespace Reviews.Infrastructure.Seeding;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken ct = default);
}
