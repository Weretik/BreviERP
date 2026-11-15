using Production.Application.Features.Staff.Shared;
using Production.Application.Features.Staff.Queries.GetWorkshopStaff;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Staff.GetWorkshopStaff;

internal static class GetWorkshopStaffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopStaff")
            .WithSummary("Workers sorted by name; archived ones only with includeArchived")
            .Produces<IReadOnlyList<WorkshopEmployeeResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, bool includeArchived = default,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetWorkshopStaffQuery(includeArchived), cancellationToken)).ToHttpResult();
}
