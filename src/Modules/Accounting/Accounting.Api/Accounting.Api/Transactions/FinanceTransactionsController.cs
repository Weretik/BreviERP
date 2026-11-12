using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Contracts;
using Accounting.Application.History;
using Accounting.Application.Journal;
using Accounting.Application.Transactions;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Transactions;

/// <summary>
/// «Бухгалтерія → Операції», «Потребують перевірки» (<c>needsReview</c>) and the operations of «Домашня бухгалтерія».
/// 409 on a stale version.
/// </summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/transactions")]
public sealed class FinanceTransactionsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceTransactions")]
    public async Task<ActionResult<FinanceTransactionPageResponse>> List(
        [FromQuery] LedgerScope scope = LedgerScope.Current,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] CategoryDirection? type = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? subcategoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] long? minMinor = null,
        [FromQuery] long? maxMinor = null,
        [FromQuery] MovementSource? source = null,
        [FromQuery] bool includeVoided = false,
        [FromQuery] bool needsReview = false,
        [FromQuery] CounterpartyKind? counterpartyKind = null,
        [FromQuery] string? counterpartyId = null,
        [FromQuery] FinanceJournalSort sort = FinanceJournalSort.Date,
        [FromQuery] FinanceSortDirection direction = FinanceSortDirection.Desc,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceTransactionsQuery(
            new FinanceTransactionFilter(scope, from, to, accountId, type, categoryId, subcategoryId, search, minMinor,
                maxMinor, source, includeVoided, needsReview,
                counterpartyKind is { } kind && !string.IsNullOrWhiteSpace(counterpartyId)
                    ? CounterpartyLink.Of(kind, counterpartyId)
                    : null), sort, direction, page), cancellationToken));

    [HttpPost(Name = "createFinanceTransaction")]
    public async Task<ActionResult<FinanceTransactionResponse>> Create([FromBody] FinanceTransactionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CreateFinanceTransactionCommand(input.Scope, input.ToFields(), AccountingAccess.By(User)), cancellationToken));

    [HttpPut("{id:guid}", Name = "updateFinanceTransaction")]
    public async Task<ActionResult<FinanceTransactionResponse>> Update(Guid id,
        [FromBody] FinanceTransactionUpdateInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateFinanceTransactionCommand(id, input.ToFields(), input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpPost("{id:guid}/void", Name = "voidFinanceTransaction")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceTransactionResponse>> Void(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new VoidFinanceTransactionCommand(id, input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpPut("{id:guid}/review", Name = "reviewFinanceTransaction")]
    public async Task<ActionResult<FinanceTransactionResponse>> Review(Guid id, [FromBody] FinanceTransactionReviewInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ReviewFinanceTransactionCommand(id, input.CategoryId, input.Counterparty,
            input.OneoffName, input.Version, AccountingAccess.By(User), input.RememberCategory), cancellationToken));

    [HttpPost("{id:guid}/promote-oneoff", Name = "promoteFinanceOneoff")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceTransactionResponse>> PromoteOneoff(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new PromoteFinanceOneoffCommand(id, input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpGet("{id:guid}/history", Name = "getFinanceTransactionHistory")]
    public async Task<ActionResult<IReadOnlyList<FinanceHistoryEntryResponse>>> History(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Transaction, id), cancellationToken));
}
