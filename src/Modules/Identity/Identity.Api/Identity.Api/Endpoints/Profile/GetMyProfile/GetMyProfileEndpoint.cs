using Identity.Application.Features.Profile.Queries.GetProfile;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.GetMyProfile;

internal static class GetMyProfileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getMyProfile").Produces<ProfileResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetProfileQuery(), cancellationToken)).ToHttpResult(ProfileResponse.From);
}
