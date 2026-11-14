using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

/// <summary>«Операції»: 50 rows per page, newest first by default, with totals of the whole selection.</summary>
public sealed record GetFinanceTransactionsQuery(FinanceTransactionFilter Filter, FinanceJournalSort Sort,
    FinanceSortDirection Direction, int Page) : IQuery<Result<FinanceTransactionPageResponse>>;
