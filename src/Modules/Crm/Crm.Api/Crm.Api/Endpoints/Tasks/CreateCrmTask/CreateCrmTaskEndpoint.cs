using Crm.Api.Access;
using Crm.Application.Features.Tasks.Commands.CreateTask;
using Crm.Application.Features.Tasks.Shared;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.CreateCrmTask;

internal static class CreateCrmTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createCrmTask")
            .WithSummary("Director or Admin gives a new task to an active Manager")
            .WithDescription("Director or Admin gives a new task to an active Manager. `version` must be 0.")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<TaskDetailResponse>(StatusCodes.Status201Created);

    private static async Task<IResult> HandleAsync(SaveTaskInput input, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(user);
        if (actor is null) return Results.Forbid();
        var result = await sender.Send(new CreateTaskCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("getCrmTask", new { id = result.Value.Id }, result.Value)
            : result.ToHttpResult();
    }
}
