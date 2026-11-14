using Content.Api.Access;

namespace Content.Api.Endpoints.ProductPages;

/// <summary>«Контент → Товари»: site pages of Catalog products (CNT-002).</summary>
internal static class ContentProductsEndpoints
{
    public static IEndpointRouteBuilder MapContentProductsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/products").RequireAuthorization(ContentAccess.ProductsPolicy)
            .WithTags("ContentProducts");
        GetContentProductsEndpoint.Map(group);
        GetContentProductEndpoint.Map(group);
        SaveContentProductEndpoint.Map(group);
        SetContentProductVisibilityEndpoint.Map(group);
        ImportOldSiteProductPagesEndpoint.Map(group);
        DeleteContentProductEndpoint.Map(group);
        return app;
    }
}
