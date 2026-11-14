using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Statements;
using Accounting.Application;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace Accounting.Application.Features.Statements.Commands.UpdateBankLine;

/// <summary>
/// The director's choice for one line of a statement that is not booked yet: category, counterparty or a one-off
/// name, description, note and whether to book it. The bank data stay as they came.
/// </summary>
public sealed record UpdateBankLineCommand(
    Guid StatementId,
    Guid LineId,
    Guid? CategoryId,
    FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName,
    string? Description,
    string? Comment,
    bool Excluded) : ICommand<Result<BankStatementLineResponse>>;
