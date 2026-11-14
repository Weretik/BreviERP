using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Reports;

/// <summary>«Резервна копія»: every finance record in one JSON file, downloaded.</summary>
internal static class FinanceExportEndpoints
{
    public static IEndpointRouteBuilder MapFinanceExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/export").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceExport");
        GetFinanceExportEndpoint.Map(group);
        return app;
    }
}
