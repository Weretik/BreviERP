using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.SiteSettings.Queries.GetPublicSiteSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Api.PublicSiteSettings;

/// <summary>
/// Site-wide settings of the public site (CNT-006): read-only, without sign-in. Only what every visitor sees in the
/// page source anyway: analytics ids, verification codes and the default sharing picture.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/site-settings")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicSiteSettingsController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicSiteSettings")]
    public async Task<ActionResult<PublicSiteSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicSiteSettingsQuery(), cancellationToken));
}
