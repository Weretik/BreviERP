using Content.Api.Access;
using Content.Api.Endpoints.ProductPages.DeleteContentProduct;
using Content.Api.Endpoints.ProductPages.GetContentProduct;
using Content.Api.Endpoints.ProductPages.GetContentProducts;
using Content.Api.Endpoints.ProductPages.ImportOldSiteProductPages;
using Content.Api.Endpoints.ProductPages.SaveContentProduct;
using Content.Api.Endpoints.ProductPages.SetContentProductVisibility;

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
