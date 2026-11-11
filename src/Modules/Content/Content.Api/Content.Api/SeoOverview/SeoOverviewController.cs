using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Contracts;
using Content.Application.SeoOverview;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.SeoOverview;

/// <summary>«Контент → SEO-огляд»: what the site misses for search (CNT-008).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.SeoOverviewPolicy)]
[Route("api/content/seo-overview")]
public sealed class SeoOverviewController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSeoOverview")]
    public async Task<ActionResult<SeoOverviewResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSeoOverviewQuery(), cancellationToken));
}
