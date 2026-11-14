using Production.Api.Access;
using Production.Api.Endpoints.Timesheet.GetTimesheetDay;
using Production.Api.Endpoints.Timesheet.SaveTimesheetDay;

namespace Production.Api.Endpoints.Timesheet;

/// <summary>«Робочий час»: the head of the workshop and Admin mark the hours of each worker (PRD-D41).</summary>
internal static class TimesheetEndpoints
{
    public static IEndpointRouteBuilder MapTimesheetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/timesheet").RequireAuthorization(ProductionAccess.Timesheet)
            .WithTags("Timesheet");
        GetTimesheetDayEndpoint.Map(group);
        SaveTimesheetDayEndpoint.Map(group);
        return app;
    }
}
