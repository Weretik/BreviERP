using Identity.Application.Features.Users.Commands.UpdateStaffDetails;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Users.UpdateStaffDetails;

internal static class UpdateStaffDetailsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:int}/details", HandleAsync).WithName("updateStaffDetails")
            .WithSummary("Change a staff account's name, email and phone")
            .WithDescription("A new email ends the account's sessions within 5 minutes. Empty phone clears it. Only the director changes the director (403).").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, UpdateStaffDetailsRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateStaffDetailsCommand(userId, request.FullName, request.Email, request.PhoneNumber), cancellationToken)).ToNoContentHttpResult();
}

public sealed record UpdateStaffDetailsRequest(string FullName, string Email, string? PhoneNumber);
