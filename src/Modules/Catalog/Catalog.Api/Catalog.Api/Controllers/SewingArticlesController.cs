using Catalog.Application.Features.Product.Sewing;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/v1/products/{productId:int}")]
public sealed class SewingArticlesController(ISender sender) : ControllerBase
{
    [HttpGet("articles", Name = "listSewingArticles")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(SewingArticleList), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SewingArticleList>> ListArticles(
        [FromRoute] int productId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListSewingArticlesQuery(productId), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Ok(result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("articles", Name = "createSewingArticle")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(SewingArticleView), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SewingArticleView>> CreateArticle(
        [FromRoute] int productId,
        [FromBody] CreateSewingArticleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateSewingArticleCommand(productId, request), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok)
            return this.ToActionResult(result);

        return CreatedAtRoute("listSewingArticles", new { productId }, result.Value);
    }

    /// <summary>Changes the sale SKU, activity and fabric of one fabric variant (CAT-015).</summary>
    [HttpPut("articles/{articleId:long}", Name = "updateSewingArticle")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(typeof(SewingArticleView), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SewingArticleView>> UpdateArticle(
        [FromRoute] int productId,
        [FromRoute] long articleId,
        [FromBody] UpdateSewingArticleRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateSewingArticleCommand(productId, articleId, request),
            cancellationToken));

    /// <summary>Removes a fabric variant that has no saved prices yet (CAT-015).</summary>
    [HttpDelete("articles/{articleId:long}", Name = "deleteSewingArticle")]
    [Authorize(Policy = "CanManageProducts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteArticle(
        [FromRoute] int productId,
        [FromRoute] long articleId,
        [FromQuery] long expectedVersion,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSewingArticleCommand(productId, articleId, expectedVersion),
            cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? NoContent() : this.ToActionResult(result);
    }
}
