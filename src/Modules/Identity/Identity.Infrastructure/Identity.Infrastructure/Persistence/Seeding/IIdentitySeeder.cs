namespace Identity.Infrastructure.Persistence.Seeding;

public interface IIdentitySeeder : ISeeder
{
    new Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
