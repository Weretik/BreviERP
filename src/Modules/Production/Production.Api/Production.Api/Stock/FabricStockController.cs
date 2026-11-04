using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Stock.Consumption;
using Production.Application.Stock.GetChoices;
using Production.Application.Stock.GetStock;
using Production.Application.Stock.GetStockLine;
using Production.Application.Stock.ImportStock;
using Production.Application.Stock.Movements;

namespace Production.Api.Stock;

/// <summary>«Склад тканин»: lines, movements, the period summary and the transfer from the old «Цех».</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.OrdersPolicy)]
[Route("api/production/fabric-stock")]
public sealed class FabricStockController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFabricStock")]
    public async Task<ActionResult<FabricStockListResponse>> List([FromQuery] string? search,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricStockQuery(actor, search), cancellationToken))
            : Forbid();

    /// <summary>Fabrics with a FAB- code and active suppliers for the forms.</summary>
    [HttpGet("choices", Name = "getFabricStockChoices")]
    public async Task<ActionResult<FabricStockChoicesResponse>> Choices(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricStockChoicesQuery(actor), cancellationToken))
            : Forbid();

    [HttpGet("{id:int}", Name = "getFabricStockLine")]
    public async Task<ActionResult<FabricStockLineResponse>> Get(int id, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetFabricStockLineQuery(actor, id), cancellationToken))
            : Forbid();

    /// <summary>«Початковий залишок» or «Прихід»; a new fabric and colour opens its line.</summary>
    [HttpPost("movements", Name = "createFabricMovement")]
    public async Task<ActionResult<FabricStockLineResponse>> Create(CreateFabricMovementInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateFabricMovementCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>«Коригування» to the counted metres.</summary>
    [HttpPost("{id:int}/correction", Name = "correctFabricStock")]
    public async Task<ActionResult<FabricStockLineResponse>> Correct(int id, CorrectFabricStockInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CorrectFabricStockCommand(actor, id, input), cancellationToken))
            : Forbid();

    /// <summary>«Сторно» of an opening, receipt or correction.</summary>
    [HttpPost("{id:int}/movements/{movementId:int}/reverse", Name = "reverseFabricMovement")]
    public async Task<ActionResult<FabricStockLineResponse>> Reverse(int id, int movementId,
        ReverseFabricMovementInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReverseFabricMovementCommand(actor, id, movementId, input),
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
