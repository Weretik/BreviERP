using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTaskBoard;

namespace Crm.Api.Endpoints.Tasks.GetCrmTaskBoard;

internal static class GetCrmTaskBoardEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("board", HandleAsync).WithName("getCrmTaskBoard");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        int? assigneeUserId = null, QueryEnum<TaskBoardPeriod>? period = null,
        CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetTaskBoardQuery(actor, assigneeUserId, period?.Value ?? TaskBoardPeriod.Week),
            cancellationToken)).ToHttpResult();
    }
}
