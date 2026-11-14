using Media.Application.Features.Files.Commands.UpdateMediaFile;
using Media.Application.Features.Files.Shared;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Files.UpdateMediaFile;

/// <summary>Renames the photo, changes its default alt text or folder.</summary>
internal static class UpdateMediaFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync).WithName("updateMediaFile").Produces<MediaFileResponse>();

    private static async Task<IResult> HandleAsync(int id, MediaFileInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateMediaFileCommand(id, input), cancellationToken)).ToHttpResult();
}
