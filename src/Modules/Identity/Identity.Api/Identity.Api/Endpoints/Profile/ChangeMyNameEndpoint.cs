using Identity.Application.Features.Profile.Commands.ChangeOwnName;

namespace Identity.Api.Endpoints.Profile;

internal static class ChangeMyNameEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("name", HandleAsync).WithName("changeMyName").Produces(StatusCodes.Status204NoContent);

    private static async Task<IResult> HandleAsync(ChangeNameRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ChangeOwnNameCommand(request.FullName), cancellationToken)).ToNoContentHttpResult();
}

public sealed record ChangeNameRequest(string FullName);
