using Crm.Application.Features.Tasks.Shared;
using Crm.Application.Features.Tasks.Queries.GetTaskAssignees;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.GetCrmTaskAssignees;

internal static class GetCrmTaskAssigneesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("assignees", HandleAsync).WithName("getCrmTaskAssignees")
            .WithSummary("Active users with the Manager role, by name; for the assignee picker and the manager filter")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<IReadOnlyList<TaskAssigneeResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetTaskAssigneesQuery(), cancellationToken)).ToHttpResult();
}
