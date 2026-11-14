using Accounting.Api.Access;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Api.Endpoints.Reports.GetFinanceReport;
using Accounting.Api.Endpoints.Reports.GetFinanceReportCsv;

namespace Accounting.Api.Endpoints.Reports;

/// <summary>«Бухгалтерія → Звіти»: a report on the screen and the same report as a CSV file.</summary>
internal static class FinanceReportsEndpoints
{
    public static IEndpointRouteBuilder MapFinanceReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/reports").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceReports");
        GetFinanceReportEndpoint.Map(group);
        GetFinanceReportCsvEndpoint.Map(group);
        return app;
    }
}
