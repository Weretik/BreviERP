using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;

/// <summary>
/// «Контрагенти»: one tab, 50 rows per page, with the counts of every tab for the same search. Without
/// <c>IncludeArchived</c> archived counterparties, inactive clients and suppliers, and cancelled one-off operations are
/// left out.
/// </summary>
public sealed record GetFinanceCounterpartiesQuery(FinanceCounterpartyTab Tab, string? Search, bool IncludeArchived, int Page)
    : IQuery<Result<FinanceCounterpartyPageResponse>>;
