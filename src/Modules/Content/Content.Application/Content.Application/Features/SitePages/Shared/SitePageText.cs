using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Features.SitePages.Shared;

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
