using Identity.Application.Features.Users.Commands.SetStaffBlocked;

namespace Identity.Api.Endpoints.Users;

internal static class SetStaffBlockedEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/blocked", HandleAsync).WithName("setStaffBlocked").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, SetStaffBlockedRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetStaffBlockedCommand(userId, request.Blocked), cancellationToken)).ToNoContentHttpResult();
}

public sealed record SetStaffBlockedRequest(bool Blocked);
