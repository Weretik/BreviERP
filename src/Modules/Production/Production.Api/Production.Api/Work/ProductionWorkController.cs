using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Work.GetChoices;
using Production.Application.Work.GetItemWork;
using Production.Application.Work.GetWorkLog;
using Production.Application.Work.ImportWork;
using Production.Application.Work.RecordWork;

namespace Production.Api.Work;

/// <summary>«Журнал робіт» and «Робота» of an item: Admin and the head of the workshop (PRD-D37).</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.OrdersPolicy)]
[Route("api/production")]
public sealed class ProductionWorkController(ISender sender) : ControllerBase
{
    /// <summary>Records of a month (<c>YYYY-MM</c>), optionally of one worker or order.</summary>
    [HttpGet("work", Name = "getWorkLog")]
    public async Task<ActionResult<WorkLogResponse>> List([FromQuery] string month, [FromQuery] int? employeeId,
        [FromQuery] int? orderId, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetWorkLogQuery(actor, month, employeeId, orderId), cancellationToken))
            : Forbid();

    /// <summary>Workers, items of «Нове» orders and over-plan reasons for «Додати роботу».</summary>
    [HttpGet("work/choices", Name = "getWorkChoices")]
    public async Task<ActionResult<WorkChoicesResponse>> Choices(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetWorkChoicesQuery(actor), cancellationToken))
            : Forbid();

    [HttpGet("orders/{id:int}/items/{itemId:int}/work", Name = "getProductionItemWork")]
    public async Task<ActionResult<ProductionItemWorkResponse>> Item(int id, int itemId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetProductionItemWorkQuery(actor, id, itemId), cancellationToken))
            : Forbid();

    [HttpPost("orders/{id:int}/items/{itemId:int}/work", Name = "createProductionWork")]
    public async Task<ActionResult<ProductionItemWorkResponse>> Create(int id, int itemId,
        CreateProductionWorkInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateProductionWorkCommand(actor, id, itemId, input),
                cancellationToken))
            : Forbid();

    /// <summary>«Сторно» of a record with a reason.</summary>
    [HttpPost("work/{workId:int}/void", Name = "voidProductionWork")]
    public async Task<ActionResult<ProductionItemWorkResponse>> Void(int workId, VoidProductionWorkInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new VoidProductionWorkCommand(actor, workId, input), cancellationToken))
            : Forbid();

    /// <summary>Moves the work log of the old «Цех» (Admin); <c>dryRun</c> only shows it.</summary>
    [HttpPost("work/import", Name = "importProductionWork")]
    public async Task<ActionResult<ProductionWorkImportResult>> Import(ProductionWorkImportInput input,
        [FromQuery] bool dryRun, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ImportProductionWorkCommand(actor, input.Rows, dryRun),
                cancellationToken))
            : Forbid();
}
