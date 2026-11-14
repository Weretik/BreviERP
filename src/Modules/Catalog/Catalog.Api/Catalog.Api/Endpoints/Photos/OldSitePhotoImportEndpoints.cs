using Catalog.Api.Endpoints.Photos.ImportOldSitePhotos;

namespace Catalog.Api.Endpoints.Photos;

/// <summary>Photos and missing sewing models from the old site (CAT-016); Admin only, dry run first.</summary>
internal static class OldSitePhotoImportEndpoints
{
    public static IEndpointRouteBuilder MapOldSitePhotoImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/old-site-photo-import")
            .WithTags("OldSitePhotoImport");
        ImportOldSitePhotosEndpoint.Map(group);
        return app;
    }
}
