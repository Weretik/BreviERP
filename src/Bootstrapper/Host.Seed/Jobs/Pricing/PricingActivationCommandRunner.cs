using Microsoft.Extensions.Configuration;
using SewingPricing.Jobs.Activation;

namespace Host.Seed.Jobs;

internal static class PricingActivationCommandRunner
{
    public static Task RunAsync(IServiceProvider services, IConfiguration configuration) =>
        PricingActivationCommand.RunAsync(services, configuration);
}
