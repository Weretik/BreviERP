using Identity.Application.Features.Users.Commands.SetStaffPassword;

namespace Identity.Api.Endpoints.Users.SetStaffPassword;

internal static class SetStaffPasswordEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/password", HandleAsync).WithName("setStaffPassword").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, SetStaffPasswordRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new SetStaffPasswordCommand(userId, request.Password), cancellationToken)).ToNoContentHttpResult();
}

public sealed record SetStaffPasswordRequest(string Password);
