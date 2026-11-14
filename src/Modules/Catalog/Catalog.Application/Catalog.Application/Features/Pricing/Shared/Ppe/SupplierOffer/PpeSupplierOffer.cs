using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

/// <summary>
/// The PPE card: supplier fields from the sheet tab «ЗІЗ», Insight XML prices of the current offer version, and the
/// live three-tier calculation (CAT-008).
/// </summary>
public sealed record PpeSupplierOffer(
    int ProductId,
    string? Unit,
    int? SupplierId,
    string? SupplierName,
    string? SupplierSku,
    string? SupplierUrl,
    decimal? SupplierPrice,
    bool AllowManualPrice,
    string? RuleCode,
    decimal? InsightWholesale,
    decimal? InsightRetail,
    Guid? OfferVersionId,
    DateTimeOffset? PriceUpdatedAt,
    PpePriceCalculation Calculation);
