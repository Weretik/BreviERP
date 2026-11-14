using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

namespace Catalog.Api.Controllers.Pricing.Sewing;

[ApiController]
[Route("api/v1/pricing/sewing/published")]
public sealed class SewingPublishedProductsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getPublishedSewingProducts")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<IReadOnlyList<SewingPublishedProductPrices>>> Get(
        [FromQuery] int[] productIds, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPublishedSewingProductsQuery(productIds), cancellationToken));
}
