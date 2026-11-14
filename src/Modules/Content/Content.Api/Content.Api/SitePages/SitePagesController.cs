using System.Security.Claims;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.SitePages.Commands.SaveSitePage;
using Content.Application.Features.SitePages.Queries.GetSitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Api.SitePages;

public sealed record SaveSitePageRequest(int Revision, SitePageText Uk, SitePageText Ru);

/// <summary>«Контент → Сторінки сайту»: search texts of the fixed pages of the public site (CNT-007).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.SitePagesPolicy)]
[Route("api/content/site-pages")]
public sealed class SitePagesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSitePages")]
    public async Task<ActionResult<SitePageListResponse>> GetList(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSitePagesQuery(), cancellationToken));

    /// <summary>Replaces the texts of one page: 404 for an unknown page, 409 when the revision is stale.</summary>
    [HttpPut("{key}", Name = "saveSitePage")]
    public async Task<ActionResult<SitePageResponse>> Save(string key, [FromBody] SaveSitePageRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SaveSitePageCommand(key, request.Revision, request.Uk, request.Ru, By()), cancellationToken));

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
