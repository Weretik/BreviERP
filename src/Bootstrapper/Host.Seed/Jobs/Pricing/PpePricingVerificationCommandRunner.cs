using System.Text.Json;
using CatalogImport.PpePricing.Verification;
using Microsoft.Extensions.Configuration;

namespace Host.Seed.Jobs;

internal static class PpePricingVerificationCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"] ?? throw new InvalidOperationException("PPE verification requires --report <path>.");
        var report = await PpePricingVerificationJob.RunAsync(configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."));
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"PPE diagnostic: matched={report.MatchedProducts}; incomplete={report.IncompleteProducts}; differing values={report.DifferentValues}. No input approved.");
        if (report.UnexplainedValues != 0)
            throw new InvalidDataException("PPE arithmetic differs from frozen source; inspect the report.");
    }
}
