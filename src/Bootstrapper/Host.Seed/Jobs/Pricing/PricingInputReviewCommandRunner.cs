using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SewingPricing.Jobs.Review;

namespace Host.Seed.Jobs;

internal static class PricingInputReviewCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"] ?? throw new InvalidOperationException("Input review requires --report <path>.");
        var settings = configuration["settings-source"] ?? throw new InvalidOperationException("Input review requires --settings-source <path>.");
        var ppe = configuration["ppe-source"] ?? throw new InvalidOperationException("Input review requires --ppe-source <path>.");
        var report = await PricingInputReviewJob.RunAsync(configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."), settings, ppe);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Input review: matched={report.MatchedSubjects}; different={report.DifferentSubjects}; held={report.HeldSubjects}. No inputs changed or confirmed.");
        if (report.DifferentSubjects > 0)
            throw new InvalidDataException("Current inputs differ from frozen source; inspect the report before any correction.");
    }
}
