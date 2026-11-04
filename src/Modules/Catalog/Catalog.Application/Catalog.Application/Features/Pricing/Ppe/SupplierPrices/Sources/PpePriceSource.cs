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

/// <summary>When the daily sync job updates a source by itself.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PpePriceUpdateCadence>))]
public enum PpePriceUpdateCadence
{
    /// <summary>Only a staff member updates it (files).</summary>
    Manual,
    /// <summary>Monday morning, Kyiv time.</summary>
    WeeklyMonday,
    Daily
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
    PpePriceUpdateCadence Cadence,
    /// <summary>
    /// The automatic update of a website, Google Sheets or cabinet source works: the daily job runs it on its
    /// <see cref="Cadence"/> and «Оновити зараз» is offered. Cabinet sources arrive in a later stage.
    /// </summary>
    bool AutomaticUpdateConnected)
{
    public bool AcceptsFiles => FileKind != PpePriceFileKind.None;

    /// <summary>Prices come from the supplier's site, lists or cabinet, so Brevi can fetch them itself.</summary>
    public bool CanRefresh => Method != PpePriceSourceMethod.File && AutomaticUpdateConnected;
}
