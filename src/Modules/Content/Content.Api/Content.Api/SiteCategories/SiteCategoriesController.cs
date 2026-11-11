using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Contracts;
using Content.Application.SiteCategories.Delete;
using Content.Application.SiteCategories.GetList;
using Content.Application.SiteCategories.GetOne;
using Content.Application.SiteCategories.ImportOldSite;
using Content.Application.SiteCategories.Reorder;
using Content.Application.SiteCategories.Save;
using Content.Application.SiteCategories.SetVisibility;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.SiteCategories;

/// <summary>«Контент → Розділи»: categories and subcategories of the public site catalog (CNT-003).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.CategoriesPolicy)]
[Route("api/content/categories")]
public sealed class SiteCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSiteCategories")]
    public async Task<ActionResult<SiteCategoryList>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteCategoriesQuery(), cancellationToken));

    [HttpGet("{id:int}", Name = "getSiteCategory")]
    public async Task<ActionResult<SiteCategoryResponse>> Get(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetSiteCategoryQuery(id), cancellationToken));

    /// <summary>Adds a category or a subcategory at the end of its parent: 409 when the address is taken.</summary>
    [HttpPost(Name = "createSiteCategory")]
    public async Task<ActionResult<SiteCategoryResponse>> Create([FromBody] SaveSiteCategoryRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(null, request), cancellationToken));

    /// <summary>Replaces the category: 409 when the revision is stale, the address is taken or the move is not allowed.</summary>
    [HttpPut("{id:int}", Name = "saveSiteCategory")]
    public async Task<ActionResult<SiteCategoryResponse>> Save(int id, [FromBody] SaveSiteCategoryRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(id, request), cancellationToken));

    /// <summary>«Показувати на сайті» from the tree.</summary>
    [HttpPatch("{id:int}/visibility", Name = "setSiteCategoryVisibility")]
    public async Task<ActionResult<SiteCategoryVisibilityResponse>> SetVisibility(int id,
        [FromBody] SiteCategoryVisibilityRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SetSiteCategoryVisibilityCommand(id, request.ShowOnSite, By()), cancellationToken));

    /// <summary>The new order of the categories of one parent.</summary>
    [HttpPut("order", Name = "reorderSiteCategories")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reorder([FromBody] ReorderSiteCategoriesRequest request,
        CancellationToken cancellationToken) =>
        Plain(await sender.Send(new ReorderSiteCategoriesCommand(request.ParentId, request.Ids), cancellationToken));

    /// <summary>
    /// One-time transfer of the old-site category tree with SEO, texts and products; needs the create right. Counts
    /// only unless <c>dryRun</c> is false.
    /// </summary>
    [HttpPost("old-site-import", Name = "importOldSiteCategories")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<ActionResult<OldSiteCategoryImportResponse>> ImportOldSite(
        [FromBody] ImportOldSiteCategoriesRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new ImportOldSiteCategoriesCommand(request.DryRun, request.ShowOnSite, request.Overwrite, request.Items,
                By()), cancellationToken));

    /// <summary>Removes the category; refused with 409 while it has subcategories.</summary>
    [HttpDelete("{id:int}", Name = "deleteSiteCategory")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        Plain(await sender.Send(new DeleteSiteCategoryCommand(id), cancellationToken));

    private SaveSiteCategoryCommand Command(int? id, SaveSiteCategoryRequest request) =>
        new(id, request.Revision, request.ParentId, request.Slug, request.ShowOnSite, request.Uk, request.Ru, By());

    private IActionResult Plain(Result result) => result.Status switch
    {
        ResultStatus.Ok or ResultStatus.NoContent => NoContent(),
        ResultStatus.NotFound => NotFound(),
        ResultStatus.Conflict => Conflict(result.Errors),
        ResultStatus.Invalid => BadRequest(result.ValidationErrors),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
