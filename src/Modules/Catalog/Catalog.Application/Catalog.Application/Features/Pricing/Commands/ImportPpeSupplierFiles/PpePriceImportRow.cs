using System.Text.Json.Serialization;

namespace Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;

[JsonConverter(typeof(JsonStringEnumConverter<PpePriceImportStatus>))]
public enum PpePriceImportStatus
{
    /// <summary>The new price differs; applying saves it.</summary>
    Changed,
    Unchanged,
    /// <summary>«Дозволити ручну ціну» is on, so the import keeps the price.</summary>
    ManualPrice,
    /// <summary>The product is not active, so the import keeps the price, as the Apps Script did.</summary>
    Inactive,
    NotInFile,
    Invalid
}

/// <summary>A PPE product of the supplier and what the import does with its price.</summary>
public sealed record PpePriceImportRow(
    int ProductId,
    string? Code,
    string Name,
    PpePriceImportStatus Status,
    decimal? OldPrice,
    decimal? NewPrice,
    decimal? OldWholesale,
    decimal? NewWholesale,
    decimal? OldRetail,
    decimal? NewRetail,
    string? Message);

/// <summary>A supplier PPE product with its current offer, as the planner needs it.</summary>
public sealed record PpePriceImportProduct(
    int ProductId,
    string? Code,
    string Name,
    bool IsActive,
    bool AllowManualPrice,
    string? SupplierSku,
    string? SupplierUrl,
    decimal? CurrentPrice,
    decimal? CurrentWholesale,
    decimal? CurrentRetail);
