using Media.Application.Features.Folders.Queries.GetMediaFolders;
using Media.Application.Features.Folders.Shared;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Folders.GetMediaFolders;

internal static class GetMediaFoldersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getMediaFolders").Produces<IReadOnlyList<MediaFolderResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetMediaFoldersQuery(), cancellationToken)).ToHttpResult();
}
