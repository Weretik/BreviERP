using Catalog.Application.Contracts.Pricing.Ppe;
using CatalogImport.Source;

namespace CatalogImport.PpePricing.Verification;

internal sealed record PpePricingVerificationData(
    string SpreadsheetId,
    string SourceHash,
    string SourceCapturedAt,
    IReadOnlyList<WorkbookRow> Rows,
    IReadOnlyDictionary<string, PpePricingProductInput> InputsBySku);
