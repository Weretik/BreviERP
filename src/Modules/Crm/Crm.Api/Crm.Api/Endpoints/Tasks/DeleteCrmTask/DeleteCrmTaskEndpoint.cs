using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.DeleteTask;

namespace Crm.Api.Endpoints.Tasks.DeleteCrmTask;

internal static class DeleteCrmTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", HandleAsync).WithName("deleteCrmTask")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender,
        int version = default, CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new DeleteTaskCommand(actor, id, version), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
