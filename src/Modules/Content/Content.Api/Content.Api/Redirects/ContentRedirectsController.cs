using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Contracts;
using Content.Application.Redirects.Delete;
using Content.Application.Redirects.GetList;
using Content.Application.Redirects.ImportOldSite;
using Content.Application.Redirects.Save;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Redirects;

/// <summary>«Контент → Переадресації»: 301 redirects of the public site (CNT-005).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.RedirectsPolicy)]
[Route("api/content/redirects")]
public sealed class ContentRedirectsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSiteRedirects")]
    public async Task<ActionResult<SiteRedirectList>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteRedirectsQuery(), cancellationToken));

    /// <summary>Adds a redirect: 409 when the old address already has one.</summary>
    [HttpPost(Name = "createSiteRedirect")]
    public async Task<ActionResult<SiteRedirectResponse>> Create([FromBody] SaveSiteRedirectRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(null, request), cancellationToken));

    /// <summary>Changes a redirect: 409 when the old address is taken by another one.</summary>
    [HttpPut("{id:int}", Name = "saveSiteRedirect")]
    public async Task<ActionResult<SiteRedirectResponse>> Save(int id, [FromBody] SaveSiteRedirectRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(id, request), cancellationToken));

    /// <summary>
    /// Transfer of the old-site addresses; needs the create right. Only reports unless <c>dryRun</c> is false.
    /// </summary>
    [HttpPost("old-site-import", Name = "importOldSiteRedirects")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<OldSiteRedirectImportResponse>> ImportOldSite(
        [FromBody] ImportOldSiteRedirectsRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new ImportOldSiteRedirectsCommand(request.DryRun, request.Overwrite, request.Items, By()), cancellationToken));

    [HttpDelete("{id:int}", Name = "deleteSiteRedirect")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteSiteRedirectCommand(id), cancellationToken)).Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };

    private SaveSiteRedirectCommand Command(int? id, SaveSiteRedirectRequest request) =>
        new(id, request.FromPath, request.ToPath, request.Note, By());

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
