namespace Catalog.Api.Contracts.Products.Overview;

/// <summary>A new product from the page «Новий ЗІЗ» or «Нова модель» (CAT-009).</summary>
public sealed record CreateCatalogProductRequest(
    string Type,
    string? Code,
    string Name,
    int SectionId,
    string? Season,
    bool IsActive,
    bool IsVerified,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    CreatePpeSupplierRequest? Ppe);

/// <summary>The supplier block of a new PPE product.</summary>
public sealed record CreatePpeSupplierRequest(
    int SupplierId,
    string? SupplierSku,
    string SupplierUrl,
    decimal SupplierPrice,
    bool AllowManualPrice,
    string RuleCode,
    string Unit);
