using Identity.Application.Features.Users.Commands.UpdateStaffDetails;

namespace Identity.Api.Endpoints.Users.UpdateStaffDetails;

internal static class UpdateStaffDetailsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/details", HandleAsync).WithName("updateStaffDetails").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, UpdateStaffDetailsRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateStaffDetailsCommand(userId, request.FullName, request.Email, request.PhoneNumber), cancellationToken)).ToNoContentHttpResult();
}

public sealed record UpdateStaffDetailsRequest(string FullName, string Email, string? PhoneNumber);
