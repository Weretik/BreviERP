using Content.Domain.ProductPages;

namespace Content.Application.Contracts;

/// <summary>
/// A row of «Контент → Товари». <paramref name="HasPage"/> is false when nobody has made the page yet. The
/// <c>Has…</c> flags say what the site would miss; <paramref name="OnSite"/> is true only when the mark is set and the
/// Catalog product is active. <paramref name="CategoryCount"/> is the number of site categories the product is in.
/// </summary>
public sealed record ContentProductRow(
    int ProductId,
    string Type,
    string? Code,
    string Name,
    int? SectionId,
    string? SectionName,
    bool IsActive,
    string? MainPhotoUrl,
    bool HasPrice,
    bool HasPage,
    bool ShowOnSite,
    bool OnSite,
    string? Slug,
    bool HasUkrainianText,
    bool HasRussianText,
    bool HasSeo,
    DateTimeOffset? UpdatedAtUtc,
    int CategoryCount = 0);

public sealed record ContentProductPage(IReadOnlyList<ContentProductRow> Items, int Page, int PageSize, int Total);

/// <summary>Texts of one language; empty lists and nulls when nothing is written.</summary>
public sealed record ContentProductTextResponse(
    string? Name,
    string? SeoTitle,
    string? SeoDescription,
    IReadOnlyList<ProductFact> Summary,
    IReadOnlyList<ProductTextBlock> Description,
    IReadOnlyList<ProductSpecGroup> Specs);

public sealed record ContentProductVariantResponse(
    long ArticleId,
    string SaleSku,
    bool IsActive,
    string? FabricName,
    string? FabricDetails,
    ContentPriceTiersResponse? Prices);

public sealed record ContentPriceTiersResponse(decimal Retail, decimal Wholesale, decimal LargeWholesale);

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

public sealed record ContentProductVisibilityResponse(int ProductId, bool ShowOnSite, bool OnSite, int Revision, string Slug);

/// <summary>A page of the old site that was not transferred, with the reason.</summary>
public sealed record OldSiteImportNote(string Url, string? Name, string Reason);

/// <summary>
/// Result of the old-site transfer (or what it would do when <paramref name="DryRun"/>): pages created, pages whose
/// texts were replaced, pages kept because they already had texts, and what was left out.
/// </summary>
public sealed record OldSiteImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    IReadOnlyList<OldSiteImportNote> NotMatched,
    IReadOnlyList<OldSiteImportNote> Problems);
