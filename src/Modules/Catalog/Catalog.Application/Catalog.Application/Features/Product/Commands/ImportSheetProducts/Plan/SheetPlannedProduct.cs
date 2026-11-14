using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Application.Features.Product.Commands.ImportSheetProducts.Drafts;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts.Plan;

/// <summary>A product the import writes: a new one (<paramref name="Existing"/> null) or a changed stored one.</summary>
/// <param name="Slug">The new product's slug; a stored product keeps its own.</param>
/// <param name="Offer">A PPE price version to append; null when the price and rule stay.</param>
public sealed record SheetPlannedProduct(
    ProductEntity? Existing,
    ProductId Id,
    string Slug,
    SheetProductDraft Draft,
    SheetOfferPlan? Offer);
