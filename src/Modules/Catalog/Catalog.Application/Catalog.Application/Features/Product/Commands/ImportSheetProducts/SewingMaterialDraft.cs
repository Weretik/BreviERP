using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Enums;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <param name="ArticleSku">Null for a line shared by all articles.</param>
/// <param name="ReferenceId">Reference fabric or accessory ID.</param>
public sealed record SewingMaterialDraft(
    string? ArticleSku,
    SewingMaterialKind Kind,
    int ReferenceId,
    decimal Quantity,
    SewingMaterialUnit Unit,
    string? Comment);
