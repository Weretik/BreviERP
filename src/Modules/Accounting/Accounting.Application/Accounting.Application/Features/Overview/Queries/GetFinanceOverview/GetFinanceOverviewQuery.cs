using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Balances.Shared;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview.Results;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview;

/// <summary>«Огляд»: balances, income and expense per currency for an area (<paramref name="Scope"/> null — both).</summary>
public sealed record GetFinanceOverviewQuery(LedgerScope? Scope, DateOnly From, DateOnly To)
    : IQuery<Result<FinanceOverviewResponse>>;
