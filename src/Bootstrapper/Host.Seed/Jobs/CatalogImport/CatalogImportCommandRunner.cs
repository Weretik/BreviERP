using System.Text.Json;
using CatalogImport.Execution;
using Microsoft.Extensions.Configuration;

namespace Host.Seed.Jobs;

internal static class CatalogImportCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = configuration["report"]
            ?? throw new InvalidOperationException("Catalogue import requires --report <path>.");
        var report = await CatalogImportJob.RunAsync(
            configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."),
            configuration["catalog-import"]!, configuration["mode"] ?? "validate");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Catalog import: {report.Status}; products={report.Products}; articles={report.Articles}; materials={report.Materials}; operations={report.Operations}.");
    }
}

