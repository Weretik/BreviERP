using Media.Application.Features.Files.Queries.SearchMediaFiles;
using Media.Application.Features.Files.Shared;

namespace Media.Api.Endpoints.Files;

/// <summary>Photos by name and/or folder, newest first, one page at a time.</summary>
internal static class SearchMediaFilesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("searchMediaFiles").Produces<MediaFilePage>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, int? folderId = null, string? search = null, bool includeSubfolders = true, int page = 1, int pageSize = 48, CancellationToken cancellationToken = default) =>
        (await sender.Send(new SearchMediaFilesQuery(folderId, includeSubfolders, search, page, pageSize), cancellationToken)).ToHttpResult();
}
