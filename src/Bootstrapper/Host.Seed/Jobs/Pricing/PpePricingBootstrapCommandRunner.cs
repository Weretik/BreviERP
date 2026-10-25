using System.Text.Json;
using CatalogImport.PpePricing.Bootstrap;
using Microsoft.Extensions.Configuration;
namespace Host.Seed.Jobs;

internal static class PpePricingBootstrapCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"] ?? throw new InvalidOperationException("PPE bootstrap requires --report <path>.");
        var report = await PpePricingBootstrapJob.RunAsync(configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."), configuration["ppe-pricing-bootstrap"]!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report));
        Console.WriteLine($"PPE baseline: {report.Status}; rules={report.Rules}; offers={report.Offers}.");
    }
}
