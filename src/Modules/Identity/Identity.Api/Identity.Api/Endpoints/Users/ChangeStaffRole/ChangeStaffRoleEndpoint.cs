using Identity.Application.Features.Users.Commands.ChangeStaffRole;

namespace Identity.Api.Endpoints.Users.ChangeStaffRole;

internal static class ChangeStaffRoleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/role", HandleAsync).WithName("changeStaffRole").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, ChangeStaffRoleRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeStaffRoleCommand(userId, request.Role), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeStaffRoleRequest(string Role);
