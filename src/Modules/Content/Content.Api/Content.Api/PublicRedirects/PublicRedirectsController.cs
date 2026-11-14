using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.Redirects.Queries.GetPublicRedirects;

namespace Content.Api.PublicRedirects;

/// <summary>
/// Redirects of the public site (CNT-005 stage 1): read-only, without sign-in. Each old address comes with the final
/// page, so the site answers one 301. Answers may be kept by the caller for five minutes.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/redirects")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicRedirectsController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicRedirects")]
    public async Task<ActionResult<PublicRedirectList>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicRedirectsQuery(), cancellationToken));
}
