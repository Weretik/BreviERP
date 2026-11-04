using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Orders.Files;
using Production.Application.Orders.ImportProgress;
using Production.Application.Orders.Telegram;
using Production.Application.Orders.Tracking;

namespace Production.Api.Orders;

/// <summary>Work on an order after «Передати в цех»: stages, shipments, status, files and the Telegram message.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.OrdersPolicy)]
[Route("api/production/orders")]
public sealed class ProductionOrderTrackingController(ISender sender) : ControllerBase
{
    /// <summary>Ticks the stages of an item; the furthest one colours the row.</summary>
    [HttpPut("{id:int}/items/{itemId:int}/stages", Name = "setProductionItemStages")]
    public async Task<ActionResult<ProductionOrderResponse>> SetStages(int id, int itemId,
        SetProductionItemStagesInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SetProductionItemStagesCommand(actor, id, itemId, input), cancellationToken))
            : Forbid();

    /// <summary>«Відвантажити» part of an item.</summary>
    [HttpPost("{id:int}/items/{itemId:int}/shipments", Name = "createProductionShipment")]
    public async Task<ActionResult<ProductionOrderResponse>> Ship(int id, int itemId,
        CreateProductionShipmentInput input, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CreateProductionShipmentCommand(actor, id, itemId, input), cancellationToken))
            : Forbid();

    [HttpDelete("{id:int}/shipments/{shipmentId:int}", Name = "cancelProductionShipment")]
    public async Task<ActionResult<ProductionOrderResponse>> CancelShipment(int id, int shipmentId,
        [FromQuery] int version, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CancelProductionShipmentCommand(actor, id, shipmentId, version), cancellationToken))
            : Forbid();

    /// <summary>«Закрити»: everything is shipped.</summary>
    [HttpPost("{id:int}/close", Name = "closeProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Close(int id, ProductionOrderVersionInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CloseProductionOrderCommand(actor, id, input.Version), cancellationToken))
            : Forbid();

    /// <summary>«Скасувати» with a reason.</summary>
    [HttpPost("{id:int}/cancel", Name = "cancelProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Cancel(int id, CancelProductionOrderInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new CancelProductionOrderCommand(actor, id, input), cancellationToken))
            : Forbid();

    /// <summary>«Повернути в роботу» (Admin).</summary>
    [HttpPost("{id:int}/reopen", Name = "reopenProductionOrder")]
    public async Task<ActionResult<ProductionOrderResponse>> Reopen(int id, ProductionOrderVersionInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ReopenProductionOrderCommand(actor, id, input.Version), cancellationToken))
            : Forbid();

    /// <summary>Uploads a file of the order or of one item to the CDN, in the order's folder.</summary>
    [HttpPost("{id:int}/files", Name = "uploadProductionOrderFile")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionOrderResponse>> Upload(int id,
        [FromForm] UploadProductionOrderFileRequest request, CancellationToken cancellationToken)
    {
        if (this.Actor() is not { } actor) return Forbid();
        if (request.File is null) return BadRequest(new { error = "Оберіть файл." });
        await using var content = request.File.OpenReadStream();
        return this.ToActionResult(await sender.Send(new UploadProductionOrderFileCommand(actor, id, request.ItemId,
            request.Kind, content, request.File.FileName, request.File.ContentType, request.File.Length), cancellationToken));
    }

    [HttpDelete("{id:int}/files/{fileId:int}", Name = "deleteProductionOrderFile")]
    public async Task<ActionResult<ProductionOrderResponse>> DeleteFile(int id, int fileId,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new DeleteProductionOrderFileCommand(actor, id, fileId), cancellationToken))
            : Forbid();

    /// <summary>«Надіслати в цех»: the one message to the workshop group.</summary>
    [HttpPost("{id:int}/telegram", Name = "sendProductionOrderTelegram")]
    public async Task<ActionResult<ProductionOrderResponse>> SendTelegram(int id, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SendProductionOrderTelegramCommand(actor, id), cancellationToken))
            : Forbid();

    /// <summary>Moves stages, file links and Telegram marks of the old «Цех» (Admin); <c>dryRun</c> only shows them.</summary>
    [HttpPost("progress-import", Name = "importProductionProgress")]
    public async Task<ActionResult<ProductionProgressImportResult>> ImportProgress(
        [FromBody] IReadOnlyList<ProductionProgressImportRow> rows, [FromQuery] bool dryRun,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new ImportProductionProgressCommand(actor, rows, dryRun), cancellationToken))
            : Forbid();

    /// <summary>20 MB file and the form fields around it.</summary>
    private const long MaxRequestBytes = 21 * 1024 * 1024;
}
