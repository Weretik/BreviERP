using Catalog.Application.Features.Pricing.Sewing.ArticleSummary;

namespace Catalog.Api.Controllers.Pricing.Sewing;

/// <summary>Sewing product card: live price of every Article without saving a calculation (CAT-007).</summary>
[ApiController]
[Route("api/v1/products/{productId:int}/articles/pricing-summary")]
public sealed class SewingArticleSummaryController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSewingArticleSummary")]
    [Authorize(Policy = "CatalogRead")]
    [ProducesResponseType(typeof(SewingArticleSummary), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SewingArticleSummary>> Get(int productId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSewingArticleSummaryQuery(productId), cancellationToken));
}
