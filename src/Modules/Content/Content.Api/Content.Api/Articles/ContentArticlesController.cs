using System.Security.Claims;
using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Content.Api.Access;
using Content.Application.Articles.Delete;
using Content.Application.Articles.GetList;
using Content.Application.Articles.GetOne;
using Content.Application.Articles.Save;
using Content.Application.Articles.SetVisibility;
using Content.Application.Contracts;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.Articles;

/// <summary>«Контент → Статті»: articles of the public site with their SEO and photos (CNT-004).</summary>
[ApiController]
[Authorize(Policy = ContentAccess.ArticlesPolicy)]
[Route("api/content/articles")]
public sealed class ContentArticlesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getContentArticles")]
    public async Task<ActionResult<ArticleList>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetContentArticlesQuery(), cancellationToken));

    [HttpGet("{id:int}", Name = "getContentArticle")]
    public async Task<ActionResult<ArticleResponse>> Get(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetContentArticleQuery(id), cancellationToken));

    /// <summary>Adds an article: 409 when the address is taken or a photo is not in the library.</summary>
    [HttpPost(Name = "createContentArticle")]
    public async Task<ActionResult<ArticleResponse>> Create([FromBody] SaveContentArticleRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(null, request), cancellationToken));

    /// <summary>Replaces the article: 409 when the revision is stale, the address is taken or a photo is not in the library.</summary>
    [HttpPut("{id:int}", Name = "saveContentArticle")]
    public async Task<ActionResult<ArticleResponse>> Save(int id, [FromBody] SaveContentArticleRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(Command(id, request), cancellationToken));

    /// <summary>«Показувати на сайті» from the list.</summary>
    [HttpPatch("{id:int}/visibility", Name = "setContentArticleVisibility")]
    public async Task<ActionResult<ArticleVisibilityResponse>> SetVisibility(int id,
        [FromBody] ContentArticleVisibilityRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SetContentArticleVisibilityCommand(id, request.ShowOnSite, By()), cancellationToken));

    /// <summary>Removes the article; its photos stay in the library.</summary>
    [HttpDelete("{id:int}", Name = "deleteContentArticle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteContentArticleCommand(id), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok or ResultStatus.NoContent => NoContent(),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private SaveContentArticleCommand Command(int? id, SaveContentArticleRequest request) =>
        new(id, request.Revision, request.Slug, request.PublishedOn, request.ShowOnSite, request.CoverPhotoId,
            request.Uk, request.Ru, By());

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
