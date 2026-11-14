using Production.Api.Access;

namespace Production.Api.Endpoints.Staff;

/// <summary>«Працівники цеху»: the workers the head of the workshop records work and pay for.</summary>
internal static class WorkshopStaffEndpoints
{
    public static IEndpointRouteBuilder MapWorkshopStaffEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/staff").RequireAuthorization(ProductionAccess.Staff)
            .WithTags("WorkshopStaff");
        GetWorkshopStaffEndpoint.Map(group);
        CreateWorkshopEmployeeEndpoint.Map(group);
        UpdateWorkshopEmployeeEndpoint.Map(group);
        SetWorkshopEmployeeActiveEndpoint.Map(group);
        ImportWorkshopStaffEndpoint.Map(group);
        return app;
    }
}
