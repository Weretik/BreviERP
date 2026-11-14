using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Shared;

namespace Media.Api.Endpoints.Folders.CreateMediaFolder;

internal static class CreateMediaFolderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createMediaFolder").Produces<MediaFolderResponse>();

    private static async Task<IResult> HandleAsync(MediaFolderInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CreateMediaFolderCommand(input), cancellationToken)).ToHttpResult();
}
