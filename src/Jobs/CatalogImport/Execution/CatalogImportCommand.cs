using System.Text.Json;
using Microsoft.Extensions.Configuration;
using CatalogImport.Execution.Target;

namespace CatalogImport.Execution;

/// <summary>--catalog-import=<source>: validates or imports the catalogue source and writes the report (see the job README).</summary>
public static class CatalogImportCommand
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        var path = CatalogImportReportPath.Resolve(configuration["report"]
            ?? throw new InvalidOperationException("Catalogue import requires --report <path>."),
            Environment.GetEnvironmentVariable("CLOUD_RUN_EXECUTION"));
        var sourcePath = configuration["catalog-import"]
            ?? throw new InvalidOperationException("Catalogue import requires a source path.");
        if (string.Equals(Path.GetFullPath(path), Path.GetFullPath(sourcePath),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("Catalogue report must not overwrite the source.");
        var approvedTarget = CatalogImportTargetConfiguration.Read(configuration);
        var report = await CatalogImportJob.RunAsync(
            configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:Default."),
            sourcePath, configuration["mode"] ?? "validate", approvedTarget: approvedTarget);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Catalog import: {report.Status}; products={report.Products}; articles={report.Articles}; materials={report.Materials}; operations={report.Operations}.");
    }
}

