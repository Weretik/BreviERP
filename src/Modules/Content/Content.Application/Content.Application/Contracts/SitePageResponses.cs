using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Contracts;

/// <summary>
/// Texts of a page in one language: search title and description (CNT-007), the main text at the bottom of the page and
/// its frequent questions (CNT-010). Null or empty keeps the site's own text. A save without <paramref name="Text"/> or
/// <paramref name="Faq"/> clears them; responses always list them.
/// </summary>
public sealed record SitePageText(
    string? Title,
    string? Description,
    IReadOnlyList<ProductTextBlock>? Text = null,
    IReadOnlyList<SiteFaqItem>? Faq = null);

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

/// <summary>Texts of one page for the public site; <c>text</c> and <c>faq</c> are always lists.</summary>
public sealed record PublicSitePage(string Key, SitePageText Uk, SitePageText Ru);

public sealed record PublicSitePagesResponse(IReadOnlyList<PublicSitePage> Items);
