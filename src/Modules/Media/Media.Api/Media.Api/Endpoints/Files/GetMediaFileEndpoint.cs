using Media.Application.Features.Files.Queries.GetMediaFile;
using Media.Application.Features.Files.Shared;

namespace Media.Api.Endpoints.Files;

internal static class GetMediaFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getMediaFile").Produces<MediaFileDetailResponse>();

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetMediaFileQuery(id), cancellationToken)).ToHttpResult();
}
