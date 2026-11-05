using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Contracts;
using Accounting.Application.History;
using Accounting.Application.Journal;
using Accounting.Application.Transfers;
using Accounting.Domain.Audit;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Transfers;

/// <summary>«Бухгалтерія → Перекази» and the transfers of «Домашня бухгалтерія». 409 on a stale version.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/transfers")]
public sealed class FinanceTransfersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceTransfers")]
    public async Task<ActionResult<FinanceTransferPageResponse>> List(
        [FromQuery] LedgerScope scope = LedgerScope.Current,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] string? search = null,
        [FromQuery] long? minMinor = null,
        [FromQuery] long? maxMinor = null,
        [FromQuery] MovementSource? source = null,
        [FromQuery] bool includeVoided = false,
        [FromQuery] FinanceJournalSort sort = FinanceJournalSort.Date,
        [FromQuery] FinanceSortDirection direction = FinanceSortDirection.Desc,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceTransfersQuery(
            new FinanceTransferFilter(scope, from, to, accountId, search, minMinor, maxMinor, source, includeVoided), sort,
            direction, page), cancellationToken));

    [HttpPost(Name = "createFinanceTransfer")]
    public async Task<ActionResult<FinanceTransferResponse>> Create([FromBody] FinanceTransferInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CreateFinanceTransferCommand(input.Scope, input.ToFields(), AccountingAccess.By(User)), cancellationToken));

    [HttpPut("{id:guid}", Name = "updateFinanceTransfer")]
    public async Task<ActionResult<FinanceTransferResponse>> Update(Guid id, [FromBody] FinanceTransferUpdateInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateFinanceTransferCommand(id, input.ToFields(), input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpPost("{id:guid}/void", Name = "voidFinanceTransfer")]
    public async Task<ActionResult<FinanceTransferResponse>> Void(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new VoidFinanceTransferCommand(id, input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpGet("{id:guid}/history", Name = "getFinanceTransferHistory")]
    public async Task<ActionResult<IReadOnlyList<FinanceHistoryEntryResponse>>> History(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Transfer, id), cancellationToken));
}
