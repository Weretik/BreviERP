using Accounting.Api.Access;
using Accounting.Api.Accounts;
using Accounting.Application.Contracts;
using Accounting.Application.Statements;
using Accounting.Domain.Statements;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Statements;

/// <summary>
/// «Виписки банку»: upload a statement into a preview, correct its lines and book it. Nothing reaches the books before
/// <c>book</c>; a booked statement no longer changes. 409 on a stale version.
/// </summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/statements")]
public sealed class BankStatementsController(ISender sender) : ControllerBase
{
    /// <summary>The file (at most 12 MiB) and a little room for the form around it.</summary>
    private const long MaxRequestBytes = 13 * 1024 * 1024;

    [HttpGet(Name = "getBankStatements")]
    public async Task<ActionResult<IReadOnlyList<BankStatementResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetBankStatementsQuery(), cancellationToken));

    [HttpPost(Name = "uploadBankStatement")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BankStatementResponse>> Upload([FromForm] UploadBankStatementRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0) return BadRequest(new { error = "Оберіть файл виписки." });
        await using var content = request.File.OpenReadStream();
        return this.ToActionResult(await sender.Send(
            new UploadBankStatementCommand(request.AccountId, content, request.File.FileName, AccountingAccess.By(User)),
            cancellationToken));
    }

    [HttpGet("{id:guid}", Name = "getBankStatement")]
    public async Task<ActionResult<BankStatementDetailsResponse>> Get(Guid id, [FromQuery] BankLineState? state = null,
        [FromQuery] int page = 1, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetBankStatementQuery(id, state, page), cancellationToken));

    [HttpPut("{id:guid}/lines/{lineId:guid}", Name = "updateBankStatementLine")]
    public async Task<ActionResult<BankStatementLineResponse>> UpdateLine(Guid id, Guid lineId,
        [FromBody] BankStatementLineInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateBankLineCommand(id, lineId, input.CategoryId, input.Counterparty,
            input.OneoffName, input.Description, input.Comment, input.Excluded), cancellationToken));

    [HttpPost("{id:guid}/book", Name = "bookBankStatement")]
    public async Task<ActionResult<BankStatementResponse>> Book(Guid id, [FromBody] FinanceVersionInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new BookBankStatementCommand(id, input.Version, AccountingAccess.By(User)),
            cancellationToken));
}
