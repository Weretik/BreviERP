using Media.Application.Features.Folders.Commands.DeleteMediaFolder;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Folders.DeleteMediaFolder;

/// <summary>Deletes an empty folder.</summary>
internal static class DeleteMediaFolderEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync).WithName("deleteMediaFolder");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteMediaFolderCommand(id), cancellationToken)).ToNoContentHttpResult();
}
