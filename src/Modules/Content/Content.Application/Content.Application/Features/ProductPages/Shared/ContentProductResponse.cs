using Content.Domain.ProductPages;

namespace Content.Application.Features.ProductPages.Shared;

/// <summary>
/// The editor of one product: the row, the page (revision 0 and an offered slug when there is no page yet), texts by
/// language (<c>uk</c>, <c>ru</c>), read-only Catalog facts and the site categories the product is in.
/// </summary>
public sealed record ContentProductResponse(
    ContentProductRow Product,
    int Revision,
    string Slug,
    string? UpdatedBy,
    ContentProductTextResponse Uk,
    ContentProductTextResponse Ru,
    ContentPriceTiersResponse? Prices,
    IReadOnlyList<ContentProductVariantResponse> Variants,
    IReadOnlyList<int> CategoryIds);
