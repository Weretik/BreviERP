using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Purchasing.Api.Access;
using System.Security.Claims;
using System.Text.Json;
using Purchasing.Application.Contracts;
using Purchasing.Application.Documents.Recognize;
using Purchasing.Application.Documents.Save;
using Purchasing.Application.Documents.Template;
using Purchasing.Application.Register.GetInvoices;
using Purchasing.Application.Register.GetSyncState;
using Purchasing.Application.Register.Review;
using Purchasing.Application.Register.Sync;

namespace Purchasing.Api.Register;

/// <summary>«Контрагенти → Рахунки на закупку»: the register read from the sheet «финансы».</summary>
[ApiController]
[Authorize(Policy = PurchasingAccess.Policy)]
[Route("api/purchasing/invoices")]
public sealed class PurchaseInvoicesController(ISender sender) : ControllerBase
{
    public const int DefaultPageSize = 50;

    [HttpGet(Name = "getPurchaseInvoices")]
    public async Task<ActionResult<PurchaseInvoicePage>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] PurchaseInvoiceFilter filter = PurchaseInvoiceFilter.All,
        [FromQuery] int? supplierId = null,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(
            new GetPurchaseInvoicesQuery(page, pageSize, search, filter, supplierId), cancellationToken));

    [HttpGet("sync-state", Name = "getPurchaseInvoicesSyncState")]
    public async Task<ActionResult<PurchaseSyncStateResponse>> SyncState(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPurchaseSyncStateQuery(), cancellationToken));

    /// <summary>«Оновити зараз»: 409 while another sync runs, 503 when Google cannot be read, 422 when the sheet breaks a rule.</summary>
    [HttpPost("sync", Name = "syncPurchaseInvoices")]
    [ProducesResponseType<PurchaseSyncResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseSyncResponse>> Sync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SyncPurchaseRegisterCommand(), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Unavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors }),
            ResultStatus.Error => UnprocessableEntity(new { errors = result.Errors }),
            _ => this.ToActionResult(result),
        };
    }

    [HttpGet("suppliers", Name = "getPurchaseInvoiceSuppliers")]
    public async Task<ActionResult<IReadOnlyList<PurchaseSupplierOption>>> Suppliers(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetPurchaseSuppliersQuery(), cancellationToken));

    /// <summary>
    /// «Постачальник і примітка»: 409 when the version is stale or a sync is running, 404 when the card or the supplier
    /// is not found.
    /// </summary>
    [HttpPut("{messageId}/review", Name = "reviewPurchaseInvoice")]
    public async Task<ActionResult<PurchaseInvoiceReviewResponse>> Review(string messageId,
        [FromBody] PurchaseInvoiceReviewRequest request, CancellationToken cancellationToken)
    {
        return this.ToActionResult(await sender.Send(
            new ReviewPurchaseInvoiceCommand(messageId, request.SupplierId, request.Note, request.Version, By()),
            cancellationToken));
    }

    /// <summary>
    /// «Розпізнати» / «Розпізнати знову» one file with the AI. 503 when the AI is off, has no key or reached its limit;
    /// 409 while the sheet sync runs. Problems of the answer come back with 200 and show on the card.
    /// </summary>
    [HttpPost("files/{fileId}/recognize", Name = "recognizePurchaseFile")]
    [ProducesResponseType<PurchaseRecognitionResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PurchaseRecognitionResponse>> Recognize(string fileId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RecognizePurchaseFileCommand(fileId, By()), cancellationToken);
        return result.Status == ResultStatus.Unavailable
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, new { errors = result.Errors })
            : this.ToActionResult(result);
    }

    /// <summary>One batch of «Розпізнати всі нові»: repeat while <c>remaining</c> is above 0 and there is no <c>stopReason</c>.</summary>
    [HttpPost("recognize-new", Name = "recognizeNewPurchaseFiles")]
    public async Task<ActionResult<PurchaseRecognitionBatchResponse>> RecognizeNew(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new RecognizeNewPurchaseFilesCommand(By()), cancellationToken));

    /// <summary>«Завантажити JSON»: 400 with the problems when the facts fail the checks; 409 when other facts are saved and <c>replace</c> is false.</summary>
    [HttpPut("files/{fileId}/document", Name = "savePurchaseDocument")]
    public async Task<ActionResult<PurchaseDocumentSaveResponse>> SaveDocument(string fileId,
        [FromBody] PurchaseDocumentSaveRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SaveCheckedPurchaseDocumentCommand(fileId, request.Document, request.Replace, By()), cancellationToken));

    /// <summary>«Шаблон JSON»: the saved facts, the last AI answer or empty fields, with the file's UID and link.</summary>
    [HttpGet("files/{fileId}/document-template", Name = "getPurchaseDocumentTemplate")]
    [Produces("application/json")]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    public async Task<IActionResult> DocumentTemplate(string fileId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPurchaseDocumentTemplateQuery(fileId), cancellationToken);
        return result.IsSuccess ? Content(result.Value, "application/json") : this.ToActionResult(result);
    }

    private string By() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
        ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}

/// <summary><c>Document</c> is the JSON of the facts; <c>Replace</c> confirms replacing different saved facts.</summary>
public sealed record PurchaseDocumentSaveRequest(JsonElement Document, bool Replace);

/// <summary><c>Version</c> is the <c>reviewVersion</c> the user saw; <c>SupplierId</c> null unlinks.</summary>
public sealed record PurchaseInvoiceReviewRequest(int? SupplierId, string? Note, int Version);
