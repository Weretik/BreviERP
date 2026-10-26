using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;

namespace Host.Seed.Jobs;

internal static class SeedJobDispatcher
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        if (new[] { "catalog-import", "pricing-bootstrap", "pricing-verify", "ppe-pricing-bootstrap", "ppe-pricing-verify", "pricing-readiness", "pricing-input-review", "pricing-activate" }
            .Count(key => configuration[key] is not null) > 1)
            throw new InvalidOperationException("Select exactly one explicit seed job mode.");
        if (configuration["catalog-import"] is not null)
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

