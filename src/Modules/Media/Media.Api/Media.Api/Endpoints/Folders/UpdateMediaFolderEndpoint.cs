using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Media.Application.Features.Folders.Shared;

namespace Media.Api.Endpoints.Folders;

/// <summary>Renames the folder and/or moves it under another parent.</summary>
internal static class UpdateMediaFolderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("updateMediaFolder").Produces<MediaFolderResponse>();

    private static async Task<IResult> HandleAsync(int id, MediaFolderInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateMediaFolderCommand(id, input), cancellationToken)).ToHttpResult();
}
