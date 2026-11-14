using Accounting.Api.Access;
using Accounting.Api.Endpoints.Overview.GetFinanceOverview;

namespace Accounting.Api.Endpoints.Overview;

/// <summary>«Бухгалтерія → Огляд». Without <c>scope</c> both areas are counted together.</summary>
internal static class FinanceOverviewEndpoints
{
    public static IEndpointRouteBuilder MapFinanceOverviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/overview").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("FinanceOverview");
        GetFinanceOverviewEndpoint.Map(group);
        return app;
    }
}
