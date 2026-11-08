using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Orders.CopyOrder;
using Production.Application.Orders.CreateOrder;
using Production.Application.Orders.DeleteOrder;
using Production.Application.Orders.Embroidery;
using Production.Application.Orders.GetHistory;
using Production.Application.Orders.GetOrder;
using Production.Application.Orders.GetOrders;
using Production.Application.Orders.ImportOrders;
using Production.Application.Orders.QuoteSources;
using Production.Application.Orders.UpdateOrder;

namespace Production.Api.Orders;

/// <summary>«Виробничі завдання»: orders the workshop sews, created from sent КП.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Orders)]
[Route("api/production/orders")]
public sealed class ProductionOrdersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getProductionOrders")]
    public async Task<ActionResult<ProductionOrderPageResponse>> List([FromQuery] string? search,
        [FromQuery] string? status, [FromQuery] string? manager, [FromQuery] DateOnly? dueTo,
        [FromQuery] string? stage, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        [FromQuery] long? customerId = null, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(
            new GetProductionOrdersQuery(search, status, manager, dueTo, page, pageSize, stage, customerId),
            cancellationToken));

    [HttpGet("{id:int}", Name = "getProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Detail(int id, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionOrderQuery(actor, id), cancellationToken))
            : Forbid();

    [HttpGet("{id:int}/history", Name = "getProductionOrderHistory")]
    public async Task<ActionResult<IReadOnlyList<ProductionOrderChangeResponse>>> History(int id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetProductionOrderHistoryQuery(id), cancellationToken));

    /// <summary>Sent КП the person may pass to the workshop.</summary>
    [HttpGet("quote-sources", Name = "getProductionQuoteSources")]
    public async Task<ActionResult<IReadOnlyList<ProductionQuoteSourceResponse>>> QuoteSources(
        [FromQuery] string? search, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionQuoteSourcesQuery(actor, search), cancellationToken))
            : Forbid();

    /// <summary>The КП for the order form.</summary>
    [HttpGet("quote-sources/{quoteId:long}", Name = "getProductionQuoteDraft")]
    public async Task<ActionResult<ProductionQuoteDraftResponse>> QuoteDraft(long quoteId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionQuoteDraftQuery(actor, quoteId), cancellationToken))
            : Forbid();

    /// <summary>«Розрахунок вишивки»: saved «Ціна вишивки» calculations to link to an embroidery.</summary>
    [HttpGet("embroideries", Name = "getProductionEmbroideries")]
    public async Task<ActionResult<IReadOnlyList<ProductionEmbroideryChoiceResponse>>> Embroideries(
        [FromQuery] string? search, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionEmbroideriesQuery(actor, search), cancellationToken))
            : Forbid();

    /// <summary>«Передати в цех». A repeat with the same request id returns the same order.</summary>
    [HttpPost(Name = "createProductionOrder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductionOrderResponse>> Create(CreateProductionOrderInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateProductionOrderCommand(actor, input), cancellationToken))
            : Forbid();

    [HttpPut("{id:int}", Name = "updateProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Update(int id, UpdateProductionOrderInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new UpdateProductionOrderCommand(actor, id, input), cancellationToken))
            : Forbid();

    [HttpPost("{id:int}/copy", Name = "copyProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Copy(int id, CopyProductionOrderInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CopyProductionOrderCommand(actor, id, input), cancellationToken))
            : Forbid();

    [HttpDelete("{id:int}", Name = "deleteProductionOrder")]
    public async Task<ActionResult> Delete(int id, [FromQuery] int version, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToNoContentResult(await sender.Send(new DeleteProductionOrderCommand(actor, id, version), cancellationToken))
            : Forbid();

    /// <summary>Moves orders of the old «Цех» (Admin); with <c>dryRun</c> only shows what would happen.</summary>
    [HttpPost("import", Name = "importProductionOrders")]
    public async Task<ActionResult<ProductionOrderImportResult>> Import(
        [FromBody] IReadOnlyList<ProductionOrderImportRow> rows, [FromQuery] bool dryRun,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ImportProductionOrdersCommand(actor, rows, dryRun), cancellationToken))
            : Forbid();
}
