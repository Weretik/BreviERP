using Content.Application.Contracts;
using Content.Domain.SitePages;

namespace Content.Application.SitePages.Shared;

internal static class SitePageMapping
{
    public static SitePageResponse Response(SitePageEntry entry, SitePage? page) =>
        new(entry.Key, entry.Name, entry.Path, entry.TitleHasBrand,
            Uk(page), Ru(page), page?.Revision ?? 0, page?.UpdatedAtUtc, page?.UpdatedBy);

    public static SitePageText Uk(SitePage? page) =>
        new(page?.TitleUk, page?.DescriptionUk, page?.TextUk ?? [], page?.FaqUk ?? []);

    public static SitePageText Ru(SitePage? page) =>
        new(page?.TitleRu, page?.DescriptionRu, page?.TextRu ?? [], page?.FaqRu ?? []);
}
