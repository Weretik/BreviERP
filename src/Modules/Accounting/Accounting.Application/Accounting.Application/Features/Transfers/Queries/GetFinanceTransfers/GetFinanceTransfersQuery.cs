using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transfers;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transfers.Shared;

namespace Accounting.Application.Features.Transfers.Queries.GetFinanceTransfers;

/// <summary>«Перекази»: 50 rows per page, newest first by default, with the total of the whole selection.</summary>
public sealed record GetFinanceTransfersQuery(FinanceTransferFilter Filter, FinanceJournalSort Sort,
    FinanceSortDirection Direction, int Page) : IQuery<Result<FinanceTransferPageResponse>>;
