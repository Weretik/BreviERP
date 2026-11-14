using Identity.Application.Features.Profile.Commands.RemoveOwnAvatar;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.RemoveMyAvatar;

internal static class RemoveMyAvatarEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("avatar", HandleAsync).WithName("removeMyAvatar").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new RemoveOwnAvatarCommand(), cancellationToken)).ToNoContentHttpResult();
}
