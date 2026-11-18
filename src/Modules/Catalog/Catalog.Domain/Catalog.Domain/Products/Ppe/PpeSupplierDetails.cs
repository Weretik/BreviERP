using BuildingBlocks.Domain.Identifiers.ReferenceData;
namespace Catalog.Domain.Products.Ppe;

/// <summary>The PPE card supplier fields, as in the sheet tab «ЗІЗ» (columns D, F–H, K, N, O).</summary>
public sealed record PpeSupplierDetails(
    SupplierId SupplierId,
    string? SupplierSku,
    string? SupplierUrl,
    decimal? SupplierPrice,
    bool AllowManualPrice,
    string? PricingRuleCode,
    string? Unit);
