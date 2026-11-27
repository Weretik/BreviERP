using Catalog.Api.Endpoints.Photos.GetProductPhotos;
using Catalog.Api.Endpoints.Photos.SetProductPhotos;

namespace Catalog.Api.Endpoints.Photos;

/// <summary>
/// The tab «Фото» of a product card (CAT-016). Every staff member who reads the catalogue sees it; «Товари: Редагування»
/// adds, removes, reorders photos and picks the main one (ACC-002 R-03).
/// </summary>
internal static class ProductPhotosEndpoints
{
    public static IEndpointRouteBuilder MapProductPhotosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{id:int}/photos")
            .WithTags("ProductPhotos");
        GetProductPhotosEndpoint.Map(group);
        SetProductPhotosEndpoint.Map(group);
        return app;
    }
}
