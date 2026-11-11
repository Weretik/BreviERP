using BuildingBlocks.Api.Result;
using Content.Application.Contracts;
using Content.Application.PublicArticles;
using Content.Domain.ProductPages;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Content.Api.PublicArticles;

/// <summary>
/// Articles of the public site (CNT-004 stage 4): read-only, without sign-in. Only articles marked «Показувати на сайті»
/// come back, and nothing internal. Answers may be kept by the caller for five minutes.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/articles")]
[ResponseCache(Duration = CacheSeconds, Location = ResponseCacheLocation.Any)]
public sealed class PublicArticlesController(ISender sender) : ControllerBase
{
    public const int CacheSeconds = 300;

    [HttpGet(Name = "getPublicArticles")]
    public async Task<ActionResult<PublicArticleList>> List(
        [FromQuery] string language = ProductPageRules.Ukrainian, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetPublicArticlesQuery(language), cancellationToken));

    [HttpGet("{slug}", Name = "getPublicArticle")]
    public async Task<ActionResult<PublicArticle>> One(string slug,
        [FromQuery] string language = ProductPageRules.Ukrainian, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetPublicArticleQuery(slug, language), cancellationToken));
}
