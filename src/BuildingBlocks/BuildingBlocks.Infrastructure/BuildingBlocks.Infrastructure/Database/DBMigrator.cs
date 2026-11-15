using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace BuildingBlocks.Infrastructure.Database;

/// <summary>
/// Applies the migrations of one module in the <c>Host.Seed</c> job (database rules 5.1). The session waits at most
/// <see cref="LockTimeout"/> for a lock (5.3): a migration that meets a busy table fails fast and the deploy stops,
/// instead of queuing every query of the running site behind it.
/// </summary>
public class DbMigrator<TContext>(IServiceProvider services, ILogger<DbMigrator<TContext>> logger)
    : IDatabaseMigrator
    where TContext : DbContext
{
    public const string LockTimeout = "10s";

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async ct =>
            {
                await dbContext.Database.OpenConnectionAsync(ct);
                try
                {
                    if (dbContext.Database.IsNpgsql())
                        await dbContext.Database.ExecuteSqlRawAsync($"SET lock_timeout = '{LockTimeout}'", ct);
                    await dbContext.Database.MigrateAsync(ct);
                }
                finally
                {
                    await dbContext.Database.CloseConnectionAsync();
                }
            }, cancellationToken);

            logger.LogInformation("✅ Migrations for {Context} applied successfully.", typeof(TContext).Name);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("⏹ Migration for {Context} was canceled.", typeof(TContext).Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ {Context} migration error", typeof(TContext).Name);
            throw;
        }
    }
}
