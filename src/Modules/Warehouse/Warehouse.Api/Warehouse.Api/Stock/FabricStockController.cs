using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Access;
using Warehouse.Application.Contracts;
using Warehouse.Application.Stock.Consumption;
using Warehouse.Application.Stock.GetChoices;
using Warehouse.Application.Stock.GetStock;
using Warehouse.Application.Stock.Adjustments;
using Warehouse.Application.Stock.OneOff;
using Warehouse.Application.Stock.Opening;
using Warehouse.Application.Stock.Receipts;
using Warehouse.Application.Stock.ImportStock;
using Warehouse.Application.Stock.Movements;

namespace Warehouse.Api.Stock;

/// <summary>
/// «Склад тканин»: «Залишки», «Рух тканини», «Початкові залишки», the operations of a fabric and colour pair, the period
/// summary and the transfer from the old «Цех».
/// </summary>
[ApiController]
[Authorize(Policy = WarehouseAccess.FabricStock)]
[Route("api/warehouse/fabric-stock")]
public sealed class FabricStockController(ISender sender) : ControllerBase
{
    /// <summary>«Залишки»: fabrics with every colour the stock holds, alias groups summed.</summary>
    [HttpGet(Name = "getFabricStock")]
    public async Task<ActionResult<FabricStockListResponse>> List(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricStockQuery(actor), cancellationToken))
            : Forbid();

    /// <summary>Stock lines are never deleted: their history stays; «Уточнити» brings a balance to zero.</summary>
    [HttpDelete("{id:int}", Name = "deleteFabricStock")]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public ActionResult Delete(int id) =>
        Problem(statusCode: StatusCodes.Status410Gone, title: StockApiMessages.Deleted);

    /// <summary>Active offers of «Тканини», supplier names and colours for the forms.</summary>
    [HttpGet("choices", Name = "getFabricStockChoices")]
    public async Task<ActionResult<FabricStockChoicesResponse>> Choices(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricStockChoicesQuery(actor), cancellationToken))
            : Forbid();

    /// <summary>«Рух тканини»: 50 per page, newest operation date first; <c>kind</c> All, Receipt, Issue or Opening.</summary>
    [HttpGet("movements", Name = "getFabricMovements")]
    public async Task<ActionResult<FabricMovementPageResponse>> Movements([FromQuery] int page = 1,
        [FromQuery] FabricMovementFilter kind = FabricMovementFilter.All, [FromQuery] string? fabricCode = null,
        [FromQuery] string? color = null, [FromQuery] int? itemId = null, CancellationToken cancellationToken = default) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(
                new GetFabricMovementsQuery(actor, page, kind, fabricCode, color, itemId), cancellationToken))
            : Forbid();

    /// <summary>«Початковий залишок» of a new fabric and colour; once per pair.</summary>
    [HttpPost("openings", Name = "openFabricStock")]
    public async Task<ActionResult<FabricStockOperationResponse>> Open(OpenFabricStockInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new OpenFabricStockCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>«Прихід».</summary>
    [HttpPost("receipts", Name = "receiveFabric")]
    public async Task<ActionResult<FabricStockOperationResponse>> Receive(ReceiveFabricInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReceiveFabricCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>«Разова витрата / без замовлення»; the balance may go below zero.</summary>
    [HttpPost("issues", Name = "issueFabric")]
    public async Task<ActionResult<FabricStockOperationResponse>> Issue(IssueFabricInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new IssueFabricCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>«Уточнити»: the counted total of a pair with a reason.</summary>
    [HttpPost("adjustments", Name = "adjustFabricStock")]
    public async Task<ActionResult<FabricStockOperationResponse>> Adjust(AdjustFabricStockInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new AdjustFabricStockCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>«Сторно» of an opening, receipt, correction or issue without an order.</summary>
    [HttpPost("movements/{movementId:int}/reverse", Name = "reverseFabricMovement")]
    public async Task<ActionResult<FabricStockOperationResponse>> Reverse(int movementId,
        ReverseFabricMovementInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReverseFabricMovementCommand(actor, movementId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Зведення витрати» by operation date, both days included.</summary>
    [HttpGet("consumption", Name = "getFabricConsumption")]
    public async Task<ActionResult<FabricConsumptionResponse>> Consumption([FromQuery] DateOnly from,
        [FromQuery] DateOnly to, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricConsumptionQuery(actor, from, to), cancellationToken))
            : Forbid();

    /// <summary>Moves the stock of the old «Цех» (Admin); <c>dryRun</c> only shows it.</summary>
    [HttpPost("import", Name = "importFabricStock")]
    public async Task<ActionResult<FabricStockImportResult>> Import(FabricStockImportInput input,
        [FromQuery] bool dryRun, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ImportFabricStockCommand(actor, input, dryRun), cancellationToken))
            : Forbid();
}

internal static class StockApiMessages
{
    public const string Deleted =
        "Складську позицію не видаляють, щоб зберегти історію. Уточніть залишок до нуля з причиною.";
}
