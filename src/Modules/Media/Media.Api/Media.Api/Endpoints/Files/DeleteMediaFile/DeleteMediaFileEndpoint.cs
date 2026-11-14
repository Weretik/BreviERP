using Media.Application.Features.Files.Commands.DeleteMediaFile;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Files.DeleteMediaFile;

/// <summary>Deletes the photo from the CDN; 409 while a product uses it.</summary>
internal static class DeleteMediaFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteMediaFile");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteMediaFileCommand(id), cancellationToken)).ToNoContentHttpResult();
}
