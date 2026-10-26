namespace SewingPricing.Jobs.Readiness;

public static class PricingReadinessJob
{
    public static async Task<PricingReadinessReport> RunAsync(
        string connectionString, DateTimeOffset at, CancellationToken cancellationToken = default)
    {
        var source = await ImportedPricingReadinessReader.ReadAsync(connectionString, cancellationToken);
        var ppe = await PpeReadinessReader.ReadAsync(connectionString, source.Rows.Where(x => x.Text(5) == "ЗІЗ").ToArray(), cancellationToken);
        var sewing = await SewingReadinessReader.ReadAsync(connectionString,
            source.Rows.Where(x => x.Text(5) == "Швейна продукція").ToArray(), at, cancellationToken);
        var unsupported = source.Rows.Where(x => x.Text(5) is not ("ЗІЗ" or "Швейна продукція"))
            .Select(x => new PricingReadinessItem(x.Text(0), "Unsupported", null, null, false,
                [new("UnsupportedSourceType", x.Text(5))], []));
        return new(source.SpreadsheetId, source.SourceHash, source.SourceCapturedAt, at.ToUniversalTime(),
            ppe.Concat(sewing).Concat(unsupported).OrderBy(x => x.Kind).ThenBy(x => x.SaleSku, StringComparer.Ordinal).ToArray());
    }
}
