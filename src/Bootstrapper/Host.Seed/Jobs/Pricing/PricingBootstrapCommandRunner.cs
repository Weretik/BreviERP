using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SewingPricing.Jobs.Bootstrap;

namespace Host.Seed.Jobs;

internal static class PricingBootstrapCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"]
            ?? throw new InvalidOperationException("Pricing bootstrap requires --report <path>.");
        var report = await PricingBootstrapJob.RunAsync(
            configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."),
            configuration["pricing-bootstrap"]!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report));
        Console.WriteLine($"Pricing baseline: {report.Status}; settings={report.SettingsVersionId}; reference versions={report.ReferenceVersions}.");
    }
}

