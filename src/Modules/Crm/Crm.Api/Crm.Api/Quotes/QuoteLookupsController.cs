using BuildingBlocks.Api.Result;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.GetStaff;
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
    [HttpGet("catalog", Name = "searchCrmQuoteCatalog")]
    [ProducesResponseType<QuoteCatalogPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteCatalogPageResponse>> Catalog([FromQuery] string kind,
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new SearchQuoteCatalogQuery(kind, search, page, pageSize),
            cancellationToken));

    [HttpGet("staff", Name = "getCrmQuoteStaff")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<QuoteStaffResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuoteStaffResponse>>> Staff(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteStaffQuery(), cancellationToken));
}
