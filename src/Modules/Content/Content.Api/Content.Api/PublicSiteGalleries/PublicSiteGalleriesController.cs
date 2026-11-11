using BuildingBlocks.Api.Result;
using Content.Application.Contracts;
using Content.Application.PublicSiteGalleries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.PublicSiteGalleries;

/// <summary>
/// Pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011): read-only, without sign-in. Only what
/// those pages show anyway.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/galleries")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicSiteGalleriesController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicSiteGalleries")]
    public async Task<ActionResult<PublicSiteGalleriesResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPublicSiteGalleriesQuery(), cancellationToken));
}
