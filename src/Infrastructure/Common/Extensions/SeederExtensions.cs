using Infrastructure.Common.Contracts;

namespace Infrastructure.Common.Extensions;

public static class SeederExtensions
{
    public static async Task UseAppSeeders(
        this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var seeders = services.GetServices<ISeeder>();
        var logger = services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeederRunner");

        logger.LogInformation("🌱 Launch seeders for all contexts...");

        foreach (var seeder in seeders)
        {
            var name = seeder.GetType().Name;

            logger.LogInformation("➡️ Seeding with {SeederName}...", name);

            cancellationToken.ThrowIfCancellationRequested();
            await seeder.SeedAsync(cancellationToken);

            logger.LogInformation("✅ {SeederName} seeding completed", name);
        }

        logger.LogInformation("✅ All seeders were successful.");
    }
}
