using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.SetTaskUrgent;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

internal static class SetCrmTaskUrgentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/urgent", HandleAsync).WithName("setCrmTaskUrgent")
            .WithSummary("Director or Admin drags a card into or out of «Терміново»")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<TaskDetailResponse>();

    private static async Task<IResult> HandleAsync(long id, SetTaskUrgentRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new SetTaskUrgentCommand(actor, id, request.Version, request.IsUrgent), cancellationToken)).ToHttpResult();
    }
}
