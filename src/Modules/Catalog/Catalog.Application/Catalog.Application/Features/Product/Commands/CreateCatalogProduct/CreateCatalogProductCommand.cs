namespace Catalog.Application.Features.Product.Commands.CreateCatalogProduct;

/// <summary>
/// Creates a product from the admin page «Новий ЗІЗ» or «Нова модель» (CAT-009). The server picks the ID and the
/// slug; the name is Ukrainian only. An empty sewing <paramref name="Code"/> takes the next free MOD-XXX code.
/// <paramref name="Ppe"/> is required for a PPE product and must be null for a sewing one.
/// </summary>
public sealed record CreateCatalogProductCommand(
    string Type,
    string? Code,
    string Name,
    int SectionId,
    string? Season,
    bool IsActive,
    string? CurrentSiteUrl,
    string? QuoteDescription,
    CreatePpeSupplierInput? Ppe,
    string Actor) : ICommand<Result<CreatedCatalogProduct>>;

/// <summary>The supplier block of a new PPE product (sheet tab «ЗІЗ»).</summary>
public sealed record CreatePpeSupplierInput(
    int SupplierId,
    string? SupplierSku,
    string SupplierUrl,
    decimal SupplierPrice,
    bool AllowManualPrice,
    string RuleCode,
    string Unit);

/// <param name="PriceSaved">False when the product was saved but its first PPE price version was not.</param>
public sealed record CreatedCatalogProduct(int Id, string Type, string? Code, bool PriceSaved);
