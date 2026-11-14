using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;
using Catalog.Application.Abstractions.Pricing.PpeSupplierPrices;

namespace Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

/// <summary>One row of «Імпорт цін»: the source, its logic, discount, products and last run.</summary>
public sealed record PpePriceSourceView(
    string Key,
    string Title,
    PpePriceSourceMethod Method,
    string Schedule,
    string Logic,
    decimal DiscountPercent,
    decimal DefaultDiscountPercent,
    int? SupplierId,
    string? SupplierName,
    int ProductCount,
    int ManualPriceCount,
    PpePriceFileKind FileKind,
    int FileCount,
    bool AutomaticUpdateConnected,
    DateTimeOffset? SettingsUpdatedAt,
    string? SettingsUpdatedBy,
    PpePriceImportRun? LastRun);
