namespace Catalog.Api.Endpoints.Sewing;

internal static class SewingPublishedProductsEndpoints
{
    public static IEndpointRouteBuilder MapSewingPublishedProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/pricing/sewing/published")
            .WithTags("SewingPublishedProducts");
        GetPublishedSewingProductsEndpoint.Map(group);
        return app;
    }
}
