using CatalogImport.Source;

namespace SewingPricing.Jobs.Readiness;

internal sealed record ImportedPricingReadinessData(
    string SpreadsheetId, string SourceHash, string SourceCapturedAt, IReadOnlyList<WorkbookRow> Rows);
