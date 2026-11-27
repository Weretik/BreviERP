using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.MarkTaskStep;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.MarkCrmTaskStep;

internal static class MarkCrmTaskStepEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}/steps/{stepId:long}", HandleAsync).WithName("markCrmTaskStep")
            .WithSummary("The assignee or «Завдання менеджерам» ticks or unticks one step")
            .Produces<TaskDetailResponse>();

    private static async Task<IResult> HandleAsync(long id, long stepId, MarkTaskStepRequest request,
        ClaimsPrincipal user,
        [FromServices] IAuthorizationService authorization, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = await TaskActorClaims.ReadAsync(user, authorization);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(
            new MarkTaskStepCommand(actor, id, stepId, request.Version, request.IsDone),
                cancellationToken)).ToHttpResult();
    }
}
