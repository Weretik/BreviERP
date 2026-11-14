using CatalogImport.Source;
using Catalog.Application.Abstractions.Pricing;

namespace CatalogImport.PpePricing.Verification;

internal sealed record PpePricingVerificationData(
    string SpreadsheetId,
    string SourceHash,
    string SourceCapturedAt,
    IReadOnlyList<WorkbookRow> Rows,
    IReadOnlyDictionary<string, PpePricingProductInput> InputsBySku);
