using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;

namespace Accounting.Application.Features.Reports.Shared;

/// <summary>
/// A built report: totals per currency (only income and expense, never transfers) and its rows. The operations report
/// returns at most a limited number of rows (<c>Limited</c>), with <c>TotalRows</c> the full count.
/// </summary>
public sealed record FinanceReportResponse(
    FinanceReportKind Kind,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<FinanceReportTotalResponse> Totals,
    IReadOnlyList<FinanceReportRowResponse> Rows,
    int TotalRows,
    bool Limited);
