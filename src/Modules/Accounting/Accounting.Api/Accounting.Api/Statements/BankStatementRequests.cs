using Microsoft.AspNetCore.Http;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Statements;

/// <param name="AccountId">The account the statement belongs to.</param>
/// <param name="File">The statement as the bank gave it: XLSX, XLS or CSV.</param>
public sealed record UploadBankStatementRequest(Guid AccountId, IFormFile File);

/// <summary>
/// The director's choice for a statement line: a category (or none), a counterparty or a one-off name (not both), a
/// description, a note and whether to leave the line out.
/// </summary>
public sealed record BankStatementLineInput(Guid? CategoryId, FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName, string? Description, string? Comment, bool Excluded);
