using Catalog.Application.Features.Product.Commands.SetProductPhotos;

namespace Catalog.Api.Endpoints.Photos.SetProductPhotos;

internal static class SetProductPhotosEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("setProductPhotos")
            .RequireAuthorization("RequireAdminRole");

    private static async Task<IResult> HandleAsync(int id, SetProductPhotosRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetProductPhotosCommand(id, request.MediaFileIds ?? [], request.MainMediaFileId),
                cancellationToken)).ToHttpResult();
}
