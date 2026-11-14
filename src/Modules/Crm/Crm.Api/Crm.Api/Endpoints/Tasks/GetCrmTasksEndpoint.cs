using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTasks;

namespace Crm.Api.Endpoints.Tasks;

/// <param name="filter">`Open` (default), `Overdue`, `Urgent`, `New`, `InProgress` or `Done`.</param>
internal static class GetCrmTasksEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getCrmTasks");

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        string? search = null, string? filter = null, int? assigneeUserId = null, int page = 1, int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new GetTasksQuery(actor, search, filter, assigneeUserId, page, pageSize),
                cancellationToken)).ToHttpResult();
    }
}
