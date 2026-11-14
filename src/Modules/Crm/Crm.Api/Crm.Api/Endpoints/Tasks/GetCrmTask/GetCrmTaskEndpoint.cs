using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTask;

namespace Crm.Api.Endpoints.Tasks.GetCrmTask;

internal static class GetCrmTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}", HandleAsync).WithName("getCrmTask");

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetTaskQuery(actor, id), cancellationToken)).ToHttpResult();
    }
}
