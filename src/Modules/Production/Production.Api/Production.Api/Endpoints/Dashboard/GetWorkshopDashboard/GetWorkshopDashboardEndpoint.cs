using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;
using Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Dashboard.GetWorkshopDashboard;

internal static class GetWorkshopDashboardEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getWorkshopDashboard")
            .WithSummary("«Огляд виробництва»: open orders by stage, late and ready to ship (everyone with production orders)")
            .Produces<WorkshopDashboardResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        user.Actor() is { } actor
            ? (await sender.Send(new GetWorkshopDashboardQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
