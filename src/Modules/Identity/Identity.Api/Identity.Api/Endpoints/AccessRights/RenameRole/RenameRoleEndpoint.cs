using Identity.Application.Features.Access.Commands.RenameRole;
using Identity.Api.Access;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.AccessRights.RenameRole;

internal static class RenameRoleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("roles/{role}/title", HandleAsync).WithName("renameRole")
            .WithSummary("Rename a role")
            .WithDescription("Only a role made on «Права доступу»; built-in roles keep their titles (400). A title another role has: 409. The same title again changes nothing.")
            .Produces<AccessRoleResponse>();

    private static async Task<IResult> HandleAsync(string role, RenameRoleRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RenameRoleCommand(role, request.Title ?? string.Empty), cancellationToken))
            .ToHttpResult(AccessRoleResponse.From);
}

public sealed record RenameRoleRequest(string Title);
