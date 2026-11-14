using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTaskOverview;

namespace Crm.Api.Endpoints.Tasks;

internal static class GetCrmTaskOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview", HandleAsync).WithName("getCrmTaskOverview");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        int? assigneeUserId = null, CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetTaskOverviewQuery(actor, assigneeUserId),
            cancellationToken)).ToHttpResult();
    }
}
