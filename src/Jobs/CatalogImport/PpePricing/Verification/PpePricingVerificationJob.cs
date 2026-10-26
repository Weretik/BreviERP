namespace CatalogImport.PpePricing.Verification;

public static class PpePricingVerificationJob
{
    public static async Task<PpePricingVerificationReport> RunAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var data = await PpePricingVerificationReader.ReadAsync(connectionString, cancellationToken);
        var reports = data.Rows.Select(row =>
            PpePricingDiagnosticBuilder.Build(row, data.InputsBySku.GetValueOrDefault(row.Text(0)))).ToArray();
        return new(data.SpreadsheetId, data.SourceHash, data.SourceCapturedAt, DateTimeOffset.UtcNow, reports);
    }
}
