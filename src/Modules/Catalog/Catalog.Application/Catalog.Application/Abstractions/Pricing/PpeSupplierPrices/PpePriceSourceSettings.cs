namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

/// <summary>Saved «Імпорт цін» settings of one source; a source without a row uses its defaults.</summary>
public sealed record PpePriceSourceSettings(
    string Key,
    int? SupplierId,
    decimal DiscountPercent,
    DateTimeOffset UpdatedAt,
    string UpdatedBy);
