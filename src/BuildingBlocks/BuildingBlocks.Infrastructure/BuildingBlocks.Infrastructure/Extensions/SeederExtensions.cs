using BuildingBlocks.Infrastructure.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class SeederExtensions
{
    public static async Task UseAppSeeders(this IApplicationBuilder app, CancellationToken cancellationToken = default)
    {
        await app.ApplicationServices.SeedAppAsync(cancellationToken);
    }

    public static async Task SeedAppAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var scopeServices = scope.ServiceProvider;
        var logger = scopeServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("SeederRunner");


        var allSeeders = scopeServices.GetServices<ISeeder>();
        foreach (var seeder in allSeeders.DistinctBy(s => s.GetType()))
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Виконання: {Seeder}", seeder.GetType().Name);
            await seeder.SeedAsync(scopeServices, cancellationToken);
        }
    }
}

