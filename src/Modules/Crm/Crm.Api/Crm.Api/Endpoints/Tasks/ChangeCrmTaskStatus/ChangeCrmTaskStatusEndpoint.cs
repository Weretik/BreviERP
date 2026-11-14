using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.ChangeTaskStatus;

namespace Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;

internal static class ChangeCrmTaskStatusEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/status", HandleAsync).WithName("changeCrmTaskStatus");

    private static async Task<IResult> HandleAsync(long id, ChangeTaskStatusRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new ChangeTaskStatusCommand(actor, id, request.Version, request.Status), cancellationToken)).ToHttpResult();
    }
}
