using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

/// <summary>Builds a report; the operations report keeps at most <paramref name="RowLimit"/> rows.</summary>
public sealed record GetFinanceReportQuery(FinanceReportFilter Filter, int RowLimit = FinanceReports.ScreenRows)
    : IQuery<Result<FinanceReportResponse>>;
