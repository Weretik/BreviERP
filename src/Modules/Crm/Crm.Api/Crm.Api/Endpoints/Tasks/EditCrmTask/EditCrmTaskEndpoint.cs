using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.EditTask;
using Crm.Application.Features.Tasks.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.EditCrmTask;

internal static class EditCrmTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", HandleAsync).WithName("editCrmTask")
            .WithSummary("«Завдання менеджерам» saves the side panel")
            .WithDescription("«Завдання менеджерам» saves the side panel. Steps are replaced (new ids). `version` is the loaded one.")
            .Produces<TaskDetailResponse>();

    private static async Task<IResult> HandleAsync(long id, SaveTaskInput input, ClaimsPrincipal user, [FromServices] IAuthorizationService authorization,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = await TaskActorClaims.ReadAsync(user, authorization);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new EditTaskCommand(actor, id, input), cancellationToken)).ToHttpResult();
    }
}
