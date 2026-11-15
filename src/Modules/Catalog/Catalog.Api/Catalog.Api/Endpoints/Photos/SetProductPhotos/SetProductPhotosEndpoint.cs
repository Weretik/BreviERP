using Catalog.Application.Features.Product.Shared.Photos;
using Catalog.Application.Features.Product.Commands.SetProductPhotos;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Photos.SetProductPhotos;

internal static class SetProductPhotosEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("setProductPhotos")
            .WithSummary("Saves the photos of a product")
            .WithDescription("Admin only (the Director also holds Admin). The product gets exactly these library photos in this order; photos that stay keep their alt text and visibility. mainMediaFileId null keeps the current main photo or makes the first one main.")
            .RequireAuthorization("RequireAdminRole")
            .Produces<ProductPhotoList>();

    private static async Task<IResult> HandleAsync(int id, SetProductPhotosRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(
            new SetProductPhotosCommand(id, request.MediaFileIds ?? [], request.MainMediaFileId),
                cancellationToken)).ToHttpResult();
}
