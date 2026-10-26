using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SewingPricing.Jobs.Readiness;

namespace Host.Seed.Jobs;

internal static class PricingReadinessCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"] ?? throw new InvalidOperationException("Pricing readiness requires --report <path>.");
        var report = await PricingReadinessJob.RunAsync(configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."), DateTimeOffset.UtcNow);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Pricing readiness: total={report.Items.Count}; calculation-ready={report.CalculationReadyCount}; source-issues={report.SourceIssueCount}. No data approved or published.");
    }
}
