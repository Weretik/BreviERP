
namespace BuildingBlocks.Infrastructure.Database;

public interface IDatabaseMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
}
