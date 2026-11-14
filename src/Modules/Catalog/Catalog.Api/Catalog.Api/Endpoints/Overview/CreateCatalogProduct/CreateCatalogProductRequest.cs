namespace Catalog.Api.Endpoints.Overview.CreateCatalogProduct;

/// <summary>A new product from the page «Новий ЗІЗ» or «Нова модель» (CAT-009).</summary>
public sealed record CreateCatalogProductRequest(
    string Type,
    string? Code,
    string Name,
    int SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    CreatePpeSupplierRequest? Ppe);
