using Accounting.Domain.Categories;
using Accounting.Domain.Statements;
using Accounting.Application.Features.Statements.Shared;
using Accounting.Application.Features.Statements.Shared.Responses;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement.Results;

/// <summary>
/// A statement with a page of its lines (100 per page, in file order, optionally of one <c>State</c>), the number of
/// lines in each state and what «Провести виписку» would add now.
/// </summary>
public sealed record BankStatementDetailsResponse(
    BankStatementResponse Statement,
    IReadOnlyList<BankLineStateCount> States,
    BankStatementTotals Totals,
    IReadOnlyList<BankStatementLineResponse> Lines,
    int Total,
    int Page,
    int PageSize,
    BankStatementRollback? Rollback = null);
