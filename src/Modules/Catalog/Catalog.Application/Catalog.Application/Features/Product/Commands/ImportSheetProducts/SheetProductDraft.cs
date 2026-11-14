using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>The state a product takes from one sheet row, with reference IDs already resolved.</summary>
/// <param name="Ppe">Supplier fields of a PPE product; null for sewing.</param>
/// <param name="Sewing">Articles and composition of a sewing model; null for PPE.</param>
public sealed record SheetProductDraft(
    int RowNumber,
    ProductType Type,
    string Code,
    string Name,
    ProductOverviewDetails Overview,
    PpeSupplierDetails? Ppe,
    SewingCompositionDraft? Sewing);
