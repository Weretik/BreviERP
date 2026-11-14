using Media.Application.Features.Files.Queries.GetMediaFile;
using Media.Application.Features.Files.Shared;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Files.GetMediaFile;

internal static class GetMediaFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync).WithName("getMediaFile").Produces<MediaFileDetailResponse>();

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetMediaFileQuery(id), cancellationToken)).ToHttpResult();
}
