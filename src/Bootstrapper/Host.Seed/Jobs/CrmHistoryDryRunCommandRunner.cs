using System.Text.Json;
using Crm.Application.Imports.History.Planning;
using Crm.Infrastructure.Imports.History.Source;
using Host.Seed.Jobs.CrmHistory;
using Microsoft.Extensions.Configuration;

namespace Host.Seed.Jobs;

internal static class CrmHistoryDryRunCommandRunner
{
    public static async Task RunAsync(IConfiguration configuration)
    {
        if (configuration["mode"] is not (null or "dry-run"))
            throw new InvalidOperationException("CRM history mode is read-only dry-run.");
        var freeze = Required(configuration, "crm-history-dry-run");
        var sourceRoot = Required(configuration, "source-root");
        var outputs = CrmHistoryOutputPaths.Validate(freeze, sourceRoot, Required(configuration, "report"), configuration["staging-ledger"]);
        try
        {
            var request = await new CrmHistorySourceReader().ReadAsync(freeze, sourceRoot);
            var result = new CrmHistoryDryRunPlanner().Plan(request);
            if (!result.IsSuccess)
                throw new InvalidDataException("history_dry_run_request_invalid");
            await CrmHistoryReportWriter.WriteAsync(result.Value, outputs.ReportPath, outputs.LedgerPath);
            var counts = result.Value.Counts;
            Console.WriteLine($"CRM history dry-run: {result.Value.Status}; rows={counts.SourceRows}; candidates={counts.WouldImport}; review={counts.NeedsReview}; invalid={counts.Invalid}; excluded={counts.Excluded}; databaseWrites=0.");
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            throw new InvalidDataException("history_dry_run_input_or_output_unavailable");
        }
    }

    private static string Required(IConfiguration configuration, string key)
        => !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key]! :
            throw new InvalidOperationException("CRM history requires explicit source, freeze and report paths.");
}
