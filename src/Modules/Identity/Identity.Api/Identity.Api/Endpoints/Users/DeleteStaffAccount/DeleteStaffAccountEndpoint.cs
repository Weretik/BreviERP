using Identity.Application.Features.Users.Commands.DeleteStaff;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Users.DeleteStaffAccount;

internal static class DeleteStaffAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{userId:int}", HandleAsync).WithName("deleteStaffAccount")
            .WithSummary("Delete a staff account")
            .WithDescription("Removes the account and its sessions for good. Nobody deletes their own account (400). Only the director deletes the director (403).").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteStaffCommand(userId), cancellationToken)).ToNoContentHttpResult();
}
