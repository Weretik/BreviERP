using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

/// <summary>
/// Result of the old-site category transfer (or what it would do when <paramref name="DryRun"/>): categories created,
/// categories whose texts were replaced, categories kept as they are, products added to categories, old-site
/// products without a Catalog product, and categories left out with the reason.
/// </summary>
public sealed record OldSiteCategoryImportResponse(
    bool DryRun,
    int Total,
    int Created,
    int Updated,
    int Kept,
    int ProductLinks,
    IReadOnlyList<OldSiteImportNote> NotMatched,
    IReadOnlyList<OldSiteImportNote> Problems);
