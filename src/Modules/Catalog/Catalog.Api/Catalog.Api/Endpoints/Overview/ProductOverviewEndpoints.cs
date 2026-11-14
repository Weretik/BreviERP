using Catalog.Api.Endpoints.Overview.CreateCatalogProduct;
using Catalog.Api.Endpoints.Overview.GetProductCodeCheck;
using Catalog.Api.Endpoints.Overview.GetProductOverview;
using Catalog.Api.Endpoints.Overview.GetProductOverviewItem;
using Catalog.Api.Endpoints.Overview.ImportSheetProducts;
using Catalog.Api.Endpoints.Overview.UpdateProductOverview;

namespace Catalog.Api.Endpoints.Overview;

/// <summary>The «Товари» page: one row per model (sewing) or site article (PPE) with its working fields.</summary>
internal static class ProductOverviewEndpoints
{
    public static IEndpointRouteBuilder MapProductOverviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products")
            .WithTags("ProductOverview");
        GetProductOverviewEndpoint.Map(group);
        GetProductOverviewItemEndpoint.Map(group);
        UpdateProductOverviewEndpoint.Map(group);
        CreateCatalogProductEndpoint.Map(group);
        ImportSheetProductsEndpoint.Map(group);
        GetProductCodeCheckEndpoint.Map(group);
        return app;
    }
}
