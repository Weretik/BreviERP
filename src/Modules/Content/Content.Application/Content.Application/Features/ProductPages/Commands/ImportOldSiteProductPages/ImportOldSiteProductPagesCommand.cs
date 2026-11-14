using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;

namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

/// <summary>
/// One-time transfer of old-site texts and SEO (CNT-002 SC-05). A page is matched to a Catalog product by the
/// old-site link of its card: the whole path in either language, then the last part of the path.
/// <paramref name="DryRun"/> only counts. <paramref name="Overwrite"/> replaces texts of pages that already have some;
/// without it they are kept. <paramref name="ShowOnSite"/> switches transferred pages on; it never switches a page off.
/// </summary>
public sealed record ImportOldSiteProductPagesCommand(
    bool DryRun,
    bool ShowOnSite,
    bool Overwrite,
    IReadOnlyList<OldSiteProductPage> Items,
    string UpdatedBy) : ICommand<Result<OldSiteImportResponse>>
{
    public const int MaxItems = 500;
    public const int UrlMaxLength = 500;
}
