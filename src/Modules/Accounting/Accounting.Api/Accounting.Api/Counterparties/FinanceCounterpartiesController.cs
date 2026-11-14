using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Accounting.Application.Features.Counterparties.Commands.CreateFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.MergeFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.SetFinanceCounterpartyActive;
using Accounting.Application.Features.Counterparties.Commands.UpdateFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Commands.VerifyFinanceCounterparty;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.History.Queries.GetFinanceHistory;

namespace Accounting.Api.Counterparties;

/// <summary>
/// «Бухгалтерія → Контрагенти»: finance counterparties of this module, clients of «Клієнти» and suppliers of
/// «Постачальники» (read-only here), and one-off names of operations. 409 on a stale version.
/// </summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/counterparties")]
public sealed class FinanceCounterpartiesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceCounterparties")]
    public async Task<ActionResult<FinanceCounterpartyPageResponse>> List(
        [FromQuery] FinanceCounterpartyTab tab = FinanceCounterpartyTab.Financial,
        [FromQuery] string? search = null,
        [FromQuery] bool includeArchived = false,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceCounterpartiesQuery(tab, search, includeArchived, page),
            cancellationToken));

    [HttpGet("choices", Name = "getFinanceCounterpartyChoices")]
    public async Task<ActionResult<IReadOnlyList<FinanceCounterpartyChoiceResponse>>> Choices(
        [FromQuery] string? search = null, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceCounterpartyChoicesQuery(search), cancellationToken));

    [HttpPost(Name = "createFinanceCounterparty")]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Create([FromBody] FinanceCounterpartyInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new CreateFinanceCounterpartyCommand(input.ToFields(), input.Verified, AccountingAccess.By(User)),
            cancellationToken));

    [HttpPut("{id:guid}", Name = "updateFinanceCounterparty")]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Update(Guid id,
        [FromBody] FinanceCounterpartyUpdateInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateFinanceCounterpartyCommand(id, input.ToFields(), input.Version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpPost("{id:guid}/verify", Name = "verifyFinanceCounterparty")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Verify(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new VerifyFinanceCounterpartyCommand(id, input.Version,
            AccountingAccess.By(User)), cancellationToken));

    [HttpPost("{id:guid}/archive", Name = "archiveFinanceCounterparty")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Archive(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SetFinanceCounterpartyActiveCommand(id, false, input.Version,
            AccountingAccess.By(User)), cancellationToken));

    [HttpPost("{id:guid}/restore", Name = "restoreFinanceCounterparty")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Restore(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SetFinanceCounterpartyActiveCommand(id, true, input.Version,
            AccountingAccess.By(User)), cancellationToken));

    /// <summary>Joins <paramref name="id"/> into the target; returns the target.</summary>
    [HttpPost("{id:guid}/merge", Name = "mergeFinanceCounterparty")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceCounterpartyResponse>> Merge(Guid id, [FromBody] FinanceCounterpartyMergeInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new MergeFinanceCounterpartyCommand(id, input.Version, input.TargetId,
            input.TargetVersion, AccountingAccess.By(User)), cancellationToken));

    [HttpGet("{id:guid}/history", Name = "getFinanceCounterpartyHistory")]
    public async Task<ActionResult<IReadOnlyList<FinanceHistoryEntryResponse>>> History(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Counterparty, id), cancellationToken));
}
