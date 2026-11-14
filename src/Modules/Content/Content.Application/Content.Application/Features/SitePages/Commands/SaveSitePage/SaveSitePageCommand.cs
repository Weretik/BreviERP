using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Commands.SaveSitePage;

/// <summary>
/// Replaces the texts of one page with a revision check (CNT-007 SC-02, CNT-010 SC-01); an empty text gives the page
/// back its own text on the site.
/// </summary>
public sealed record SaveSitePageCommand(
    string Key,
    int Revision,
    SitePageText Uk,
    SitePageText Ru,
    string UpdatedBy) : ICommand<Result<SitePageResponse>>;
