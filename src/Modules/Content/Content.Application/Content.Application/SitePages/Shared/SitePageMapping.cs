using Content.Application.Contracts;
using Content.Domain.SitePages;

namespace Content.Application.SitePages.Shared;

internal static class SitePageMapping
{
    public static SitePageResponse Response(SitePageEntry entry, SitePage? page) =>
        new(entry.Key, entry.Name, entry.Path, entry.TitleHasBrand,
            new SitePageText(page?.TitleUk, page?.DescriptionUk),
            new SitePageText(page?.TitleRu, page?.DescriptionRu),
            page?.Revision ?? 0, page?.UpdatedAtUtc, page?.UpdatedBy);
}
