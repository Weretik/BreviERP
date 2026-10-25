using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SewingPricing.Jobs.Verification;

namespace Host.Seed.Jobs;

internal static class PricingVerificationCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"]
            ?? throw new InvalidOperationException("Pricing verification requires --report <path>.");
        var report = await SewingPricingVerificationJob.RunAsync(
            configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."),
            configuration["pricing-verify"]!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            DateTimeOffset.UtcNow);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        if (report.Any(x => x.Differences.Count != 0))
            throw new InvalidDataException("Pricing arithmetic differs from frozen source; inspect the report.");
        Console.WriteLine($"Pricing arithmetic verified for {report.Count} Articles; no price was published.");
    }
}

