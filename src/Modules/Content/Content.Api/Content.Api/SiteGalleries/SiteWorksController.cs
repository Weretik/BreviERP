using System.Security.Claims;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Domain.SiteGalleries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Queries.GetSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Api.SiteGalleries;

/// <summary>«Контент → Наші роботи» (CNT-011).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.WorksPolicy)]
[Route("api/content/works")]
public sealed class SiteWorksController(ISender sender) : ControllerBase
{
    private const string Key = SiteGallery.Works;

    [HttpGet(Name = "getSiteWorks")]
    public async Task<ActionResult<SiteGalleryResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteGalleryQuery(Key), cancellationToken));

    /// <summary>Replaces the list as a whole: 409 when the revision is stale.</summary>
    [HttpPut(Name = "saveSiteWorks")]
    public async Task<ActionResult<SiteGalleryResponse>> Save([FromBody] SaveSiteGalleryRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveSiteGalleryCommand(Key, request.Revision, request.Items, By()),
            cancellationToken));

    /// <summary>Moves the next pictures the site keeps as its own files into the «Фото» library.</summary>
    [HttpPost("transfer", Name = "transferSiteWorks")]
    public async Task<ActionResult<SiteGalleryTransferResponse>> Transfer(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new TransferSiteGalleryCommand(Key, By()), cancellationToken));

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
