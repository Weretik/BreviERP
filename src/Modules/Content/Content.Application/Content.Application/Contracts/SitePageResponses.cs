namespace Content.Application.Contracts;

/// <summary>Search texts of a page in one language; null when the site keeps its own text.</summary>
public sealed record SitePageText(string? Title, string? Description);

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

public sealed record SitePageListResponse(IReadOnlyList<SitePageResponse> Items);

/// <summary>Texts of one page for the public site.</summary>
public sealed record PublicSitePage(string Key, SitePageText Uk, SitePageText Ru);

public sealed record PublicSitePagesResponse(IReadOnlyList<PublicSitePage> Items);
