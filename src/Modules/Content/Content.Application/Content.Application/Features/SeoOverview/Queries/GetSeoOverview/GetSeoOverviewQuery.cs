using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SitePages;
using Content.Application.Features.SeoOverview.Shared;

namespace Content.Application.Features.SeoOverview.Queries.GetSeoOverview;

/// <summary>
/// What the pages shown on the site and the site-wide settings miss for search (CNT-008 SC-01). Only pages that are
/// on the site count: products with «Показувати на сайті» whose model is active, categories shown with their parent,
/// shown articles and the fixed pages in search.
/// </summary>
public sealed record GetSeoOverviewQuery : IQuery<Result<SeoOverviewResponse>>;

public sealed class GetSeoOverviewHandler(IContentDbContext db, IContentCatalogReader catalog)
    : IQueryHandler<GetSeoOverviewQuery, Result<SeoOverviewResponse>>
{
    private const string Uk = ProductPageRules.Ukrainian;
    private const string Ru = ProductPageRules.Russian;

    /// <summary>The request list is never in search.</summary>
    private const string NotInSearch = "request";

    public async ValueTask<Result<SeoOverviewResponse>> Handle(GetSeoOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var pages = new List<SeoPage>();
        pages.AddRange(await ProductsAsync(cancellationToken));
        pages.AddRange(await CategoriesAsync(cancellationToken));
        pages.AddRange(await ArticlesAsync(cancellationToken));
        pages.AddRange(await FixedPagesAsync(cancellationToken));

        var issues = SeoChecks.Issues(pages);
        var areas = Enum.GetValues<SeoArea>()
            .Select(area => new SeoAreaSummary(area, pages.Count(x => x.Area == area),
                issues.Count(x => x.Area == area)))
            .ToList();
        return new SeoOverviewResponse(await SiteProblemsAsync(cancellationToken), areas, issues);
    }

    private async Task<IReadOnlyList<SeoSiteProblem>> SiteProblemsAsync(CancellationToken cancellationToken)
    {
        var settings = await db.SiteSeoSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        var problems = new List<SeoSiteProblem>();
        if (settings?.GoogleAnalyticsId is null && settings?.GoogleTagManagerId is null)
            problems.Add(SeoSiteProblem.NoAnalytics);
        if (settings?.GoogleSiteVerification is null) problems.Add(SeoSiteProblem.NoGoogleVerification);
        if (settings?.BingSiteVerification is null) problems.Add(SeoSiteProblem.NoBingVerification);
        if (settings?.DefaultOgPhotoId is null) problems.Add(SeoSiteProblem.NoSharePicture);
        return problems;
    }

    private async Task<IEnumerable<SeoPage>> ProductsAsync(CancellationToken cancellationToken)
    {
        var shown = await db.ProductPages.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        if (shown.Count == 0) return [];
        return (await catalog.GetAllAsync(cancellationToken))
            .Where(x => x.IsActive && shown.ContainsKey(x.Id))
            .Select(product =>
            {
                var page = shown[product.Id];
                var uk = page.TextIn(Uk);
                var ru = page.TextIn(Ru);
                var name = uk?.Name ?? product.Name;
                return new SeoPage(SeoArea.Product, product.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    name, $"/product/{page.Slug}",
                    new SeoTexts(uk?.SeoTitle, uk?.SeoDescription, uk?.SeoTitle ?? name),
                    ru?.Name is null ? null : new SeoTexts(ru.SeoTitle, ru.SeoDescription, ru.SeoTitle ?? ru.Name));
            });
    }

    private async Task<IEnumerable<SeoPage>> CategoriesAsync(CancellationToken cancellationToken)
    {
        var shown = await db.SiteCategories.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        var ids = shown.Select(x => x.Id).ToHashSet();
        return shown.Where(x => x.ParentId is null || ids.Contains(x.ParentId.Value)).Select(category =>
        {
            var uk = category.TextIn(Uk);
            var ru = category.TextIn(Ru);
            var name = uk?.Name ?? category.Slug;
            return new SeoPage(SeoArea.Category, category.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                name, $"/catalog/{category.Slug}",
                new SeoTexts(uk?.SeoTitle, uk?.SeoDescription, uk?.SeoTitle ?? name),
                ru?.Name is null ? null : new SeoTexts(ru.SeoTitle, ru.SeoDescription, ru.SeoTitle ?? ru.Name));
        });
    }

    private async Task<IEnumerable<SeoPage>> ArticlesAsync(CancellationToken cancellationToken)
    {
        var shown = await db.Articles.AsNoTracking().Include(x => x.Texts).Where(x => x.ShowOnSite)
            .OrderByDescending(x => x.PublishedOn).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        return shown.Select(article =>
        {
            var uk = article.TextIn(Uk);
            var ru = article.TextIn(Ru);
            var name = uk?.Title ?? article.Slug;
            return new SeoPage(SeoArea.Article, article.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                name, $"/articles/{article.Slug}",
                new SeoTexts(uk?.SeoTitle, uk?.SeoDescription, uk?.SeoTitle ?? name),
                ru?.Title is null ? null : new SeoTexts(ru.SeoTitle, ru.SeoDescription, ru.SeoTitle ?? ru.Title));
        });
    }

    private async Task<IEnumerable<SeoPage>> FixedPagesAsync(CancellationToken cancellationToken)
    {
        var saved = await db.SitePages.AsNoTracking().ToDictionaryAsync(x => x.Key, StringComparer.Ordinal,
            cancellationToken);
        return SitePageCatalog.Pages.Where(x => x.Key != NotInSearch).Select(entry =>
        {
            var page = saved.GetValueOrDefault(entry.Key);
            // An empty text keeps the site's own, so only what staff saved is checked; the name stands in for
            // the title when looking for repeats.
            return new SeoPage(SeoArea.Page, entry.Key, entry.Name, entry.Path,
                new SeoTexts(page?.TitleUk, page?.DescriptionUk, page?.TitleUk ?? entry.Name),
                new SeoTexts(page?.TitleRu, page?.DescriptionRu, page?.TitleRu ?? $"ru:{entry.Name}"));
        });
    }
}
