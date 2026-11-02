using System.Text.Json;
using Crm.Application.Imports.History.Contracts;

namespace Host.Seed.Jobs.CrmHistory;

internal static class CrmHistoryReportWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task WriteAsync(CrmHistoryDryRunReport report, string reportPath, string? ledgerPath)
    {
        await WriteNewAsync(reportPath, report with { StagingLedger = [] });
        if (ledgerPath is not null)
            await WriteNewAsync(ledgerPath, new { schemaVersion = 1, datasetId = report.DatasetId, entries = report.StagingLedger });
    }

    private static async Task WriteNewAsync<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await JsonSerializer.SerializeAsync(output, value, JsonOptions);
            File.Move(temporary, path, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
