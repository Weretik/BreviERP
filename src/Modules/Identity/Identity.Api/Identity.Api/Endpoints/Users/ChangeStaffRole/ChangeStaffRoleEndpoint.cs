using Identity.Application.Features.Users.Commands.ChangeStaffRole;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Users.ChangeStaffRole;

internal static class ChangeStaffRoleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/role", HandleAsync).WithName("changeStaffRole")
            .WithSummary("Change a staff account's role")
            .WithDescription("The account's sessions pick up the new role within 5 minutes. Only the director changes their own role; an administrator gets 400. Only the director changes the director (403). There is at most one director (409).").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, ChangeStaffRoleRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeStaffRoleCommand(userId, request.Role), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeStaffRoleRequest(string Role);
