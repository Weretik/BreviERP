using Identity.Application.Features.Access.Commands.DeleteRole;

namespace Identity.Api.Endpoints.AccessRights;

internal static class DeleteRoleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("roles/{role}", HandleAsync).WithName("deleteRole").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(string role, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteRoleCommand(role), cancellationToken)).ToNoContentHttpResult();
}
