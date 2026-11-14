using Catalog.Application.Features.Product.Queries.GetProductPhotos;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Photos.GetProductPhotos;

internal static class GetProductPhotosEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getProductPhotos")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductPhotosQuery(id), cancellationToken)).ToHttpResult();
}
