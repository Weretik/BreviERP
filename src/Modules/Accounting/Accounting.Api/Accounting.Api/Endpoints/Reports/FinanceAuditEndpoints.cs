using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Reports;

/// <summary>«Бухгалтерія → Журнал змін»: changes of all finance records, newest first.</summary>
internal static class FinanceAuditEndpoints
{
    public static IEndpointRouteBuilder MapFinanceAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/audit").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceAudit");
        GetFinanceAuditEndpoint.Map(group);
        return app;
    }
}
