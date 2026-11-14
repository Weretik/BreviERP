using Identity.Application.Features.Users.Commands.DeleteStaff;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Users.DeleteStaffAccount;

internal static class DeleteStaffAccountEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{userId:int}", HandleAsync).WithName("deleteStaffAccount").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(int userId, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteStaffCommand(userId), cancellationToken)).ToNoContentHttpResult();
}
