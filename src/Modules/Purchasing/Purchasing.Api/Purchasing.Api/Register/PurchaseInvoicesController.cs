using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Purchasing.Api.Access;
using System.Security.Claims;
using Purchasing.Application.Contracts;
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
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetPurchaseInvoicesQuery(page, pageSize, search, filter), cancellationToken));

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
        var reviewer = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        return this.ToActionResult(await sender.Send(
            new ReviewPurchaseInvoiceCommand(messageId, request.SupplierId, request.Note, request.Version, reviewer),
            cancellationToken));
    }
}

/// <summary><c>Version</c> is the <c>reviewVersion</c> the user saw; <c>SupplierId</c> null unlinks.</summary>
public sealed record PurchaseInvoiceReviewRequest(int? SupplierId, string? Note, int Version);
