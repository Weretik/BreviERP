using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTaskOverview;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.GetCrmTaskOverview;

internal static class GetCrmTaskOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview", HandleAsync).WithName("getCrmTaskOverview")
            .WithSummary("Get CRM task overview")
            .WithDescription("Counts for the metric strip and filter chips (narrowed by `assigneeUserId` for Admin) and the workload of each active Manager (always all of them for Admin, only the actor for a Manager).")
            .Produces<TaskOverviewResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        int? assigneeUserId = null, CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetTaskOverviewQuery(actor, assigneeUserId),
            cancellationToken)).ToHttpResult();
    }
}
