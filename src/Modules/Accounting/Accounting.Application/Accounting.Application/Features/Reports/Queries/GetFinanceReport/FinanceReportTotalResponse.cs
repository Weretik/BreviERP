using System.Text.Json.Serialization;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Reports.Queries.GetFinanceReport;

public sealed record FinanceReportTotalResponse(string Currency, int Operations, long IncomeMinor, long ExpenseMinor,
    long ResultMinor);
