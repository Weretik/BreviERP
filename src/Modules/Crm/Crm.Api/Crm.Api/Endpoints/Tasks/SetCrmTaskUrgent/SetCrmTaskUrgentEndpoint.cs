using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.SetTaskUrgent;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

internal static class SetCrmTaskUrgentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/urgent", HandleAsync).WithName("setCrmTaskUrgent")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync(long id, SetTaskUrgentRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new SetTaskUrgentCommand(actor, id, request.Version, request.IsUrgent), cancellationToken)).ToHttpResult();
    }
}
