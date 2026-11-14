using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Features.SitePages.Shared;

/// <summary>
/// One fixed page of «Контент → Сторінки сайту» (CNT-007). <paramref name="TitleHasBrand"/> is true for the home page,
/// whose title is shown as written; the site adds « — Brevi» to the others. <paramref name="Revision"/> is 0 before
/// the first save.
/// </summary>
public sealed record SitePageResponse(
    string Key,
    string Name,
    string Path,
    bool TitleHasBrand,
    SitePageText Uk,
    SitePageText Ru,
    int Revision,
    DateTimeOffset? UpdatedAtUtc,
    string? UpdatedBy);
