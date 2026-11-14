using Production.Api.Access;
using Production.Api.Endpoints.Reports.GetWorkshopReport;

namespace Production.Api.Endpoints.Reports;

/// <summary>«Звіти цеху» (PRD-D48): Admin only; the director also holds Admin.</summary>
internal static class WorkshopReportsEndpoints
{
    public static IEndpointRouteBuilder MapWorkshopReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/reports").RequireAuthorization(ProductionAccess.Reports)
            .WithTags("WorkshopReports");
        GetWorkshopReportEndpoint.Map(group);
        return app;
    }
}
