using Accounting.Application.Features.Reports.Shared;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Api.Endpoints.Shared;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

namespace Accounting.Api.Endpoints.Reports.Shared;

/// <summary>The query of a report and of its CSV: the same filter.</summary>
internal static class FinanceReportFilters
{
    public static FinanceReportFilter Of(FinanceReportKind kind, DateOnly from, DateOnly to, LedgerScope? scope,
        Guid? accountId, CategoryDirection? type, Guid? categoryId, CounterpartyKind? counterpartyKind,
        string? counterpartyId) =>
        new(kind, from, to, scope, accountId, type, categoryId, AccountingQueryLink.Of(counterpartyKind,
            counterpartyId));
}
