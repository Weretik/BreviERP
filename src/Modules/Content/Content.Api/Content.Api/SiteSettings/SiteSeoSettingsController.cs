using System.Security.Claims;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;
using Content.Application.Features.SiteSettings.Queries.GetSiteSeoSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Api.SiteSettings;

public sealed record SaveSiteSeoSettingsRequest(
    int Revision,
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    int? DefaultOgPhotoId);

/// <summary>«Контент → Налаштування SEO сайту» (CNT-006).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.SeoSettingsPolicy)]
[Route("api/content/seo-settings")]
public sealed class SiteSeoSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSiteSeoSettings")]
    public async Task<ActionResult<SiteSeoSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken));

    /// <summary>Replaces the settings: 409 when the revision is stale or the picture is not in the library.</summary>
    [HttpPut(Name = "saveSiteSeoSettings")]
    public async Task<ActionResult<SiteSeoSettingsResponse>> Save([FromBody] SaveSiteSeoSettingsRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveSiteSeoSettingsCommand(request.Revision, request.GoogleAnalyticsId,
            request.GoogleTagManagerId, request.GoogleSiteVerification, request.BingSiteVerification,
            request.DefaultOgPhotoId, By()), cancellationToken));

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
