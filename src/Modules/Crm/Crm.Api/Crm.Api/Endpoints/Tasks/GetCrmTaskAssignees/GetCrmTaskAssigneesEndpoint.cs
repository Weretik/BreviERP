using Crm.Application.Features.Tasks.Queries.GetTaskAssignees;
using BuildingBlocks.Api.Errors;

namespace Crm.Api.Endpoints.Tasks.GetCrmTaskAssignees;

internal static class GetCrmTaskAssigneesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("assignees", HandleAsync).WithName("getCrmTaskAssignees")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetTaskAssigneesQuery(), cancellationToken)).ToHttpResult();
}
