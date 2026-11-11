using BuildingBlocks.Api.Result;
using Content.Application.Contracts;
using Content.Application.PublicSitePages;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.PublicSitePages;

/// <summary>
/// Search texts of the fixed pages of the public site (CNT-007): read-only, without sign-in. Only what every page
/// shows in its source anyway: the title and the description.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/site-pages")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicSitePagesController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicSitePages")]
    public async Task<ActionResult<PublicSitePagesResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicSitePagesQuery(), cancellationToken));
}
