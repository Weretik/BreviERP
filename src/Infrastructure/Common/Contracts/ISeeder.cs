namespace Infrastructure.Common.Contracts;

public interface ISeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

