using System.Text.Json.Serialization;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

/// <summary>How a supplier's prices reach Brevi.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PpePriceSourceMethod>))]
public enum PpePriceSourceMethod
{
    Website,
    GoogleSheets,
    SupplierCabinet,
    File
}

/// <summary>The file a staff member uploads for a file supplier.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PpePriceFileKind>))]
public enum PpePriceFileKind
{
    None,
    /// <summary>Three Insight YML price lists: dealer, wholesale and retail, in any order.</summary>
    InsightYml,
    /// <summary>The СЕМТО price list PDF with a text layer.</summary>
    CemtoPdf
}

/// <summary>
/// One supplier price source from the client's Apps Script: where the price comes from, when it updates and the
/// default discount off the supplier's price. The discount and the supplier link are changed in «Імпорт цін».
/// </summary>
public sealed record PpePriceSource(
    string Key,
    string Title,
    IReadOnlyList<string> SupplierAliases,
    PpePriceSourceMethod Method,
    string Schedule,
    string Logic,
    decimal DefaultDiscountPercent,
    PpePriceFileKind FileKind,
    int FileCount,
    /// <summary>Automatic updates of website, Google Sheets and cabinet sources arrive in later stages.</summary>
    bool AutomaticUpdateConnected)
{
    public bool AcceptsFiles => FileKind != PpePriceFileKind.None;
}
