using BuildingBlocks.Api.Result;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.GetCatalogProducts;
using Crm.Application.Quotes.GetStaff;
using Crm.Application.Quotes.Introductions.Contracts;
using Crm.Application.Quotes.Introductions.Setup;
using Crm.Application.Quotes.SearchCatalog;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

/// <summary>What the КП editor picks from: Catalog products and managers.</summary>
[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quote-lookups")]
public sealed class QuoteLookupsController(ISender sender) : ControllerBase
{
    /// <param name="kind">`Sewing` or `Ppe`.</param>
    /// <param name="season">`Summer` or `Winter`: only sewing models of that season.</param>
    [HttpGet("catalog", Name = "searchCrmQuoteCatalog")]
    [ProducesResponseType<QuoteCatalogPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteCatalogPageResponse>> Catalog([FromQuery] string kind,
        [FromQuery] string? search, [FromQuery] string? season, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new SearchQuoteCatalogQuery(kind, search, page, pageSize, season),
            cancellationToken));

    /// <summary>Catalog products by id, with their fabric variants and photo (at most 20).</summary>
    [HttpGet("products", Name = "getCrmQuoteCatalogProducts")]
    [ProducesResponseType<IReadOnlyList<QuoteCatalogProductResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<QuoteCatalogProductResponse>>> Products(
        [FromQuery] int[] ids, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteCatalogProductsQuery(ids), cancellationToken));

    /// <summary>What «КП для знайомства» starts from: the main model of each season and the letter variants.</summary>
    [HttpGet("introduction", Name = "getCrmQuoteIntroductionSetup")]
    [ProducesResponseType<QuoteIntroductionSetupResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteIntroductionSetupResponse>> Introduction(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteIntroductionSetupQuery(), cancellationToken));

    /// <summary>
    /// «Виріб з нуля»: prices each fabric with the chosen accessories and operations by the Catalog formula. Nothing is
    /// saved; a fabric that cannot be priced lists its problems.
    /// </summary>
    [HttpPost("custom-sewing-price", Name = "calculateCrmQuoteCustomSewing")]
    [ProducesResponseType<QuoteCustomSewingPriceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuoteCustomSewingPriceResponse>> CustomSewingPrice(
        [FromBody] QuoteCustomSewingPriceInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CalculateQuoteCustomSewingQuery(input), cancellationToken));

    [HttpGet("staff", Name = "getCrmQuoteStaff")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<QuoteStaffResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuoteStaffResponse>>> Staff(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteStaffQuery(), cancellationToken));
}
