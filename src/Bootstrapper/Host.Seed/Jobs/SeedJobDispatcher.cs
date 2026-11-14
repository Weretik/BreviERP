using Host.Seed.Jobs.Sync;
using Microsoft.Extensions.Configuration;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.Seeding;
using Host.Seed.Jobs.Pricing.Ppe;

namespace Host.Seed.Jobs;

internal static class SeedJobDispatcher
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (configuration["catalog-import"] is null &&
            new[] { "CatalogImport:TargetHost", "CatalogImport:TargetDatabase", "CatalogImport:SourceSha256", "CatalogImport:ApprovedConnection" }
                .Any(key => configuration[key] is not null))
            throw new InvalidOperationException("Catalogue target approval requires the explicit import mode.");
        if (configuration["sync"] is null && configuration["sync-tasks"] is not null)
            throw new InvalidOperationException("Sync task selection requires the explicit sync mode.");
        if (new[] { "catalog-import", "pricing-bootstrap", "pricing-verify", "ppe-pricing-bootstrap", "ppe-pricing-verify", "pricing-readiness", "pricing-input-review", "pricing-activate", "reminders-dispatch", "sync" }
            .Count(key => configuration[key] is not null) > 1)
            throw new InvalidOperationException("Select exactly one explicit seed job mode.");
        if (configuration["reminders-dispatch"] is not null)
            await RemindersDispatchCommandRunner.RunAsync(services);
        else if (configuration["sync"] is not null)
            await SyncJobRunner.RunAsync(services, configuration);
        else if (configuration["catalog-import"] is not null)
            await CatalogImportCommandRunner.RunAsync(configuration);
        else if (configuration["pricing-bootstrap"] is not null)
            await PricingBootstrapCommandRunner.RunAsync(configuration);
        else if (configuration["pricing-verify"] is not null)
            await PricingVerificationCommandRunner.RunAsync(configuration);
        else if (configuration["ppe-pricing-bootstrap"] is not null)
            await PpePricingBootstrapCommandRunner.RunAsync(configuration);
        else if (configuration["ppe-pricing-verify"] is not null)
            await PpePricingVerificationCommandRunner.RunAsync(configuration);
        else if (configuration["pricing-readiness"] is not null)
            await PricingReadinessCommandRunner.RunAsync(configuration);
        else if (configuration["pricing-input-review"] is not null)
            await PricingInputReviewCommandRunner.RunAsync(configuration);
        else if (configuration["pricing-activate"] is not null)
            await PricingActivationCommandRunner.RunAsync(services, configuration);
        else
        {
            await services.MigrateAppAsync();
            await services.SeedAppAsync();
        }
    }
}

