using Crm.Application.Features.Tasks.Shared;
using Crm.Api.Access;
using Crm.Application.Features.Tasks.Queries.GetTaskBoard;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Crm.Api.Endpoints.Tasks.GetCrmTaskBoard;

internal static class GetCrmTaskBoardEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("board", HandleAsync).WithName("getCrmTaskBoard")
            .WithSummary("«Мої завдання» kanban: every open task and those done in `period` (at most 500 cards)")
            .WithDescription("«Мої завдання» kanban: every open task and those done in `period` (at most 500 cards). Open cards come first, then done ones; each group is newest added first; «Завдання менеджерам» groups them by `lane`.")
            .Produces<TaskBoardResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user,
        [FromServices] IAuthorizationService authorization, [FromServices] ISender sender,
        int? assigneeUserId = null, QueryEnum<TaskBoardPeriod>? period = null,
        CancellationToken cancellationToken = default)
    {
        var actor = await TaskActorClaims.ReadAsync(user, authorization);
        if (actor is null) return Results.Forbid();
        return (await sender.Send(new GetTaskBoardQuery(actor, assigneeUserId, period?.Value ?? TaskBoardPeriod.Week),
            cancellationToken)).ToHttpResult();
    }
}
