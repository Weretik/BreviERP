using BuildingBlocks.Api.Result;
using Crm.Api.Quotes.Requests;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Crm.Api.Access;
using Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;
using Crm.Application.Features.Quotes.Queries.CalculateQuoteCustomSewing;
using Crm.Application.Features.Quotes.Queries.GetQuoteCatalogProducts;
using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;
using Crm.Application.Features.Quotes.Queries.GetQuoteIntroductionSetup;
using Crm.Application.Features.Quotes.Queries.GetQuoteModelComposition;
using Crm.Application.Features.Quotes.Queries.GetQuoteStaff;
using Crm.Application.Features.Quotes.Queries.SearchQuoteCatalog;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Introductions;

namespace Crm.Api.Quotes;

/// <summary>What the КП editor picks from: Catalog products and managers.</summary>
[ApiController]
[Authorize(Policy = CrmAccess.QuotesView)]
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

    /// <summary>
    /// The manager line under each КП fabric: composition, density, price per metre and supplier (at most 200).
    /// Fabrics that no longer exist are missing.
    /// </summary>
    [HttpGet("fabrics", Name = "getCrmQuoteFabricFacts")]
    [ProducesResponseType<IReadOnlyList<QuoteFabricFactsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<QuoteFabricFactsResponse>>> Fabrics(
        [FromQuery] int[] ids, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteFabricFactsQuery(ids), cancellationToken));

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

    /// <summary>
    /// «Змінити склад»: the composition of a Catalog sewing model (the fabric of each variant, shared accessories,
    /// services, extra fabrics and operations) to start a changed model from.
    /// </summary>
    [HttpGet("catalog/{productId:int}/composition", Name = "getCrmQuoteModelComposition")]
    [ProducesResponseType<QuoteModelCompositionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteModelCompositionResponse>> ModelComposition(int productId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteModelCompositionQuery(productId), cancellationToken));

    [HttpGet("staff", Name = "getCrmQuoteStaff")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<QuoteStaffResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<QuoteStaffResponse>>> Staff(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteStaffQuery(), cancellationToken));

    /// <summary>«Змінити фото → Завантажити своє»: stores the photo in the CDN folder of КП photos (R-34).</summary>
    [HttpPost("photos", Name = "uploadCrmQuotePhoto")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxPhotoRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxPhotoRequestBytes)]
    [ProducesResponseType<QuotePhotoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QuotePhotoResponse>> UploadPhoto([FromForm] UploadQuotePhotoRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null) return BadRequest(new { error = "Оберіть фото." });
        await using var content = request.File.OpenReadStream();
        return this.ToActionResult(await sender.Send(new UploadQuotePhotoCommand(content, request.File.FileName,
            request.File.ContentType, request.File.Length), cancellationToken));
    }

    /// <summary>Hard ceiling for the multipart body; the photo size limit of the library is checked on upload.</summary>
    private const long MaxPhotoRequestBytes = 32L * 1024 * 1024;
}
