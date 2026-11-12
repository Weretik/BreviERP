using Accounting.Api.Access;
using Accounting.Api.Results;
using Accounting.Application.Accounts;
using Accounting.Application.Contracts;
using Accounting.Application.History;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Accounts;

/// <summary>«Бухгалтерія → Рахунки і каси». 409 on a stale version: someone saved first.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/accounts")]
public sealed class FinanceAccountsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceAccounts")]
    public async Task<ActionResult<IReadOnlyList<FinanceAccountResponse>>> List([FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceAccountsQuery(includeArchived), cancellationToken));

    [HttpPost(Name = "createFinanceAccount")]
    public async Task<ActionResult<FinanceAccountResponse>> Create([FromBody] FinanceAccountInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CreateFinanceAccountCommand(input.ToFields(), AccountingAccess.By(User)),
            cancellationToken));

    [HttpPut("{id:guid}", Name = "updateFinanceAccount")]
    public async Task<ActionResult<FinanceAccountResponse>> Update(Guid id, [FromBody] FinanceAccountUpdateInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateFinanceAccountCommand(id, input.ToFields(), input.Version, AccountingAccess.By(User)), cancellationToken));

    [HttpPut("{id:guid}/cutover", Name = "setFinanceAccountCutover")]
    public async Task<ActionResult<FinanceAccountResponse>> Cutover(Guid id, [FromBody] FinanceAccountCutoverInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SetCutoverBalanceCommand(id, input.BalanceMinor, input.Confirmed,
            input.Version, AccountingAccess.By(User)), cancellationToken));

    [HttpPost("{id:guid}/archive", Name = "archiveFinanceAccount")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceAccountResponse>> Archive(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SetFinanceAccountActiveCommand(id, false, input.Version, AccountingAccess.By(User)), cancellationToken));

    [HttpPost("{id:guid}/restore", Name = "restoreFinanceAccount")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<FinanceAccountResponse>> Restore(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new SetFinanceAccountActiveCommand(id, true, input.Version, AccountingAccess.By(User)), cancellationToken));

    [HttpDelete("{id:guid}", Name = "deleteFinanceAccount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id, [FromQuery] int version, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteFinanceAccountCommand(id, version, AccountingAccess.By(User)),
            cancellationToken));

    [HttpGet("{id:guid}/history", Name = "getFinanceAccountHistory")]
    public async Task<ActionResult<IReadOnlyList<FinanceHistoryEntryResponse>>> History(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceHistoryQuery(AuditEntity.Account, id), cancellationToken));
}
