using Content.Domain.ProductPages;
using Content.Domain.SitePages;

namespace Content.Application.Features.SitePages.Shared;

/// <summary>Texts of one page for the public site; <c>text</c> and <c>faq</c> are always lists.</summary>
public sealed record PublicSitePage(string Key, SitePageText Uk, SitePageText Ru);
