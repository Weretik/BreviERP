using Media.Application.Features.Files.Commands.MoveMediaFiles;
using Media.Application.Features.Files.Shared;

namespace Media.Api.Endpoints.Files;

/// <summary>Moves several photos into one folder.</summary>
internal static class MoveMediaFilesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("move", HandleAsync).WithName("moveMediaFiles")
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(MoveMediaFilesInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new MoveMediaFilesCommand(input), cancellationToken)).ToNoContentHttpResult();
}
