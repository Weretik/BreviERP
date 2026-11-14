using Production.Api.Access;

namespace Production.Api.Endpoints.Dashboard;

/// <summary>«Огляд виробництва» on the admin home (PRD-D50): everyone with production orders.</summary>
internal static class WorkshopDashboardEndpoints
{
    public static IEndpointRouteBuilder MapWorkshopDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/dashboard").RequireAuthorization(ProductionAccess.Orders)
            .WithTags("WorkshopDashboard");
        GetWorkshopDashboardEndpoint.Map(group);
        return app;
    }
}
