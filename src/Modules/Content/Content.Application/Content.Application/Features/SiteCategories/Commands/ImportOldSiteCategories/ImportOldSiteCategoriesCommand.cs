using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

namespace Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

/// <summary>
/// One-time transfer of the old-site category tree with SEO, texts and products (CNT-003 SC-05). A category is
/// matched by its address; a product by the old-site link of its Catalog card, as in the product transfer.
/// <paramref name="DryRun"/> only counts. <paramref name="Overwrite"/> replaces texts of categories that already
/// exist; without it they are kept. Products are added to a category and never removed from it.
/// <paramref name="ShowOnSite"/> switches transferred categories on; it never switches one off. A repeated run
/// continues where the previous one stopped.
/// </summary>
public sealed record ImportOldSiteCategoriesCommand(
    bool DryRun,
    bool ShowOnSite,
    bool Overwrite,
    IReadOnlyList<OldSiteCategory> Items,
    string UpdatedBy) : ICommand<Result<OldSiteCategoryImportResponse>>
{
    public const int MaxItems = 300;
    public const int MaxProducts = 1_000;
    public const int UrlMaxLength = 500;
}
