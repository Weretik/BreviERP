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
using Warehouse.Application.Stock.Issues;
using Warehouse.Application.Stock.Adjustments;
using Warehouse.Application.Stock.OneOff;
using Warehouse.Application.Stock.Opening;
using Warehouse.Application.Stock.Receipts;
using Warehouse.Application.Stock.Movements;
using Warehouse.Application.Stock.Revisions;
using Warehouse.Application.Stock.Transfer;

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

    /// <summary>
    /// «Рух тканини»: 50 per page, newest operation date first; filters by kind, fabric, colour, item, text and
    /// operation-date period, with the count of every kind.
    /// </summary>
    [HttpGet("movements", Name = "getFabricMovements")]
    public async Task<ActionResult<FabricMovementPageResponse>> Movements([FromQuery] int page = 1,
        [FromQuery] FabricMovementFilter kind = FabricMovementFilter.All, [FromQuery] string? fabricCode = null,
        [FromQuery] string? color = null, [FromQuery] int? itemId = null, [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(
                new GetFabricMovementsQuery(actor, page, kind, fabricCode, color, itemId, search, from, to),
                cancellationToken))
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

    /// <summary>Edits an opening, receipt, correction or issue without an order; both lines are restated.</summary>
    [HttpPut("movements/{movementId:int}", Name = "editFabricMovement")]
    public async Task<ActionResult<FabricStockOperationResponse>> Edit(int movementId, EditFabricMovementInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new EditFabricMovementCommand(actor, movementId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Скасувати»: the movement stays in the history with no effect on the balance.</summary>
    [HttpPost("movements/{movementId:int}/void", Name = "voidFabricMovement")]
    public async Task<ActionResult<FabricStockOperationResponse>> Void(int movementId, VoidFabricMovementInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new VoidFabricMovementCommand(actor, movementId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Скасувати останню правку»: the state before the last edit or void comes back.</summary>
    [HttpPost("movements/{movementId:int}/restore", Name = "restoreFabricMovement")]
    public async Task<ActionResult<FabricStockOperationResponse>> Restore(int movementId,
        RestoreFabricMovementInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new RestoreFabricMovementCommand(actor, movementId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Історія правок» of a movement, newest first.</summary>
    [HttpGet("movements/{movementId:int}/revisions", Name = "getFabricMovementHistory")]
    public async Task<ActionResult<FabricMovementHistoryResponse>> History(int movementId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricMovementHistoryQuery(actor, movementId),
                cancellationToken))
            : Forbid();

    /// <summary>«Видача на завдання»: open items found by order number, client, model or fabric; newest first.</summary>
    [HttpGet("issue-items", Name = "getFabricIssueItems")]
    public async Task<ActionResult<FabricIssueItemsResponse>> IssueItems([FromQuery] string? search,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricIssueItemsQuery(actor, search), cancellationToken))
            : Forbid();

    /// <summary>The fabric card of an item: plan by part, issued, reserved and the issues with plan and fact.</summary>
    [HttpGet("issue-items/{orderId:int}/{itemId:int}", Name = "getFabricIssueItem")]
    public async Task<ActionResult<ProductionItemFabricResponse>> IssueItem(int orderId, int itemId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionItemFabricQuery(actor, orderId, itemId),
                cancellationToken))
            : Forbid();

    /// <summary>«Видача» by actual cutting: products cut now and the real pairs; one package per operation id.</summary>
    [HttpPost("issue-items/{orderId:int}/{itemId:int}/issues", Name = "issueFabricToItem")]
    public async Task<ActionResult<ProductionItemFabricResponse>> IssueToItem(int orderId, int itemId,
        CreateFabricIssueInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateFabricIssueCommand(actor, orderId, itemId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Оновити план»: the saved plan of the item takes today's norm.</summary>
    [HttpPost("issue-items/{orderId:int}/{itemId:int}/plan", Name = "refreshFabricItemPlan")]
    public async Task<ActionResult<ProductionItemFabricResponse>> RefreshPlan(int orderId, int itemId,
        RefreshItemFabricPlanInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new RefreshItemFabricPlanCommand(actor, orderId, itemId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Зведення витрати» by operation date, both days included.</summary>
    [HttpGet("consumption", Name = "getFabricConsumption")]
    public async Task<ActionResult<FabricConsumptionResponse>> Consumption([FromQuery] DateOnly from,
        [FromQuery] DateOnly to, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricConsumptionQuery(actor, from, to), cancellationToken))
            : Forbid();

    /// <summary>
    /// «Перенесення з v133» (Admin): replaces the stock with the D1 export of the old «Цех». <c>dryRun</c> checks and
    /// rolls back; <c>confirmReplace</c> allows removing records entered in Brevi itself.
    /// </summary>
    [HttpPost("transfer", Name = "transferFabricStock")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<StockTransferResult>> Transfer(StockTransferInput input, [FromQuery] bool dryRun,
        [FromQuery] bool confirmReplace, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new TransferStockCommand(actor, input, dryRun, confirmReplace),
                cancellationToken))
            : Forbid();
}

internal static class StockApiMessages
{
    public const string Deleted =
        "Складську позицію не видаляють, щоб зберегти історію. Уточніть залишок до нуля з причиною.";
}
