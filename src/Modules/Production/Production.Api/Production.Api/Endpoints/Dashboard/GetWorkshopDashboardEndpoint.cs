using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

namespace Production.Api.Endpoints.Dashboard;

internal static class GetWorkshopDashboardEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopDashboard");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetWorkshopDashboardQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
