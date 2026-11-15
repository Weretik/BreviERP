using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.EditTask;
using Crm.Application.Features.Tasks.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.EditCrmTask;

internal static class EditCrmTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("editCrmTask")
            .WithSummary("Director or Admin saves the side panel")
            .WithDescription("Director or Admin saves the side panel. Steps are replaced (new ids). `version` is the loaded one.")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<TaskDetailResponse>();

    private static async Task<IResult> HandleAsync(long id, SaveTaskInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new EditTaskCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
