using Production.Application.Features.Staff.Queries.GetWorkshopStaff;

namespace Production.Api.Endpoints.Staff;

internal static class GetWorkshopStaffEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopStaff");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, bool includeArchived = default,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetWorkshopStaffQuery(includeArchived), cancellationToken)).ToHttpResult();
}
