using System.Security.Claims;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Contracts;
using Content.Application.SiteGalleries.Get;
using Content.Application.SiteGalleries.Save;
using Content.Application.SiteGalleries.Transfer;
using Content.Domain.SiteGalleries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.SiteGalleries;

/// <summary>«Контент → Сертифікати» (CNT-011).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.CertificatesPolicy)]
[Route("api/content/certificates")]
public sealed class SiteCertificatesController(ISender sender) : ControllerBase
{
    private const string Key = SiteGallery.Certificates;

    [HttpGet(Name = "getSiteCertificates")]
    public async Task<ActionResult<SiteGalleryResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteGalleryQuery(Key), cancellationToken));

    /// <summary>Replaces the list as a whole: 409 when the revision is stale.</summary>
    [HttpPut(Name = "saveSiteCertificates")]
    public async Task<ActionResult<SiteGalleryResponse>> Save([FromBody] SaveSiteGalleryRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveSiteGalleryCommand(Key, request.Revision, request.Items, By()),
            cancellationToken));

    /// <summary>Moves the next pictures the site keeps as its own files into the «Фото» library.</summary>
    [HttpPost("transfer", Name = "transferSiteCertificates")]
    public async Task<ActionResult<SiteGalleryTransferResponse>> Transfer(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new TransferSiteGalleryCommand(Key, By()), cancellationToken));

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
