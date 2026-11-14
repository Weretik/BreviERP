using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Domain.Statements;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Statements.Shared;

namespace Accounting.Application.Features.Statements.Queries.GetBankStatement;

/// <summary>A statement with a page of its lines (100 in file order, optionally of one state) and its totals.</summary>
public sealed record GetBankStatementQuery(Guid Id, BankLineState? State, int Page)
    : IQuery<Result<BankStatementDetailsResponse>>;
